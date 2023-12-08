// Copyright © 2023 aubiyko. All rights reserved.
//
// This file is part of ccwc.
//
// ccwc is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// ccwc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ccwc.  If not, see <https://www.gnu.org/licenses/>.

using System.Buffers;
using System.Text;

namespace ccwc;

/// <summary>
/// Bytes or text cumulative statistics.
/// </summary>
/// <remarks>This class is thread-safe, except for <see cref="Reset"/> method.</remarks>
public class Counter
{
    readonly int _bufferSize;

    /// <summary>
    /// Gets or sets the value indicating whether to perform byte count.
    /// </summary>
    public bool CountBytes { get; set; }

    ulong _bytes;

    /// <summary>
    /// Gets the amount of bytes.
    /// </summary>
    public ulong Bytes => _bytes;

    /// <summary>
    /// Gets or sets the value indicating whether to perform line count.
    /// </summary>
    /// <remarks>See <see cref="Lines"/> for line definition.</remarks>
    public bool CountNewLines { get; set; }

    ulong _lines;

    /// <summary>
    /// Gets the amount of lines.
    /// A line is defined as a sequence of bytes ending on CR, LF or CR & LF.
    /// </summary>
    public ulong Lines => _lines;

    /// <summary>
    /// Gets or sets the value indicating whether to perform word count.
    /// </summary>
    /// <remarks>See <see cref="Words"/> for word definition.</remarks>
    public bool CountWords { get; set; }

    ulong _words;

    /// <summary>
    /// Gets the amount of words.
    /// A word is the sequence of non-whitespace bytes.
    /// </summary>
    public ulong Words => _words;

    /// <summary>
    /// Gets or sets the value indicating whether to perform character count.
    /// </summary>
    public bool CountCharacters { get; set; }

    ulong _chars;

    /// <summary>
    /// Gets the amount of characters.
    /// </summary>
    public ulong Characters => _chars;

    /// <summary>
    /// Initializes a new instance of the <see cref="Counter"/> class.
    /// </summary>
    /// <param name="bufferSize">A read buffer size, in bytes.</param>
    public Counter(int bufferSize = 1024) => _bufferSize = bufferSize;

    /// <summary>
    /// Clears the statistics.
    /// </summary>
    public void Reset() => _bytes = _lines = _words = _chars = 0;

    /// <summary>
    /// Calculates and cumulates the statistics for the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">An input stream.</param>
    public void CountFor(Stream stream) => CountFor(stream, Encoding.Default);

    /// <summary>
    /// Calculates and cumulates the statistics for the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">An input stream.</param>
    /// <param name="encoding">A character encoding.</param>
    public void CountFor(Stream stream, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
        ThrowIfNoMode();

        if (!CountWords && !CountCharacters)
        {
            if (!CountNewLines)
            {
                TryUseLength(stream);
            }
            else
            {
                CountFast(stream);
            }
        }
        else
        {
            CountSlow(stream, encoding);
        }
    }

    void ThrowIfNoMode()
    {
        if (!CountBytes && !CountNewLines && !CountWords && !CountCharacters)
        {
            throw new InvalidOperationException();
        }
    }

    void TryUseLength(Stream stream)
    {
        try
        {
            ulong len = (ulong)stream.Length;
            Interlocked.Add(ref _bytes, len);
        }
        catch
        {
            CountFast(stream);
        }
    }

    void CountFast(Stream stream)
    {
        bool countLines = CountNewLines;
        byte[] buf = ArrayPool<byte>.Shared.Rent(_bufferSize);
        Span<byte> span = buf;

        try
        {
            bool skipLF = false;

            int read = stream.Read(span);
            while (read > 0)
            {
                Interlocked.Add(ref _bytes, (ulong)read);

                if (countLines)
                {
                    skipLF = CountLines(span.Slice(0, read), skipLF);
                }

                read = stream.Read(span);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
    }

    bool CountLines(ReadOnlySpan<byte> buf, bool skipLF)
    {
        const byte CR = (byte)'\r';
        const byte LF = (byte)'\n';

        for (int i = 0, len = buf.Length; i < len; ++i)
        {
            byte c = buf[i];
            if (c == CR)
            {
                Interlocked.Increment(ref _lines);
                skipLF = true;
            }
            else if (c == LF)
            {
                if (!skipLF)
                {
                    Interlocked.Increment(ref _lines);
                }
                skipLF = false;
            }
        }

        return skipLF;
    }

    void CountSlow(Stream stream, Encoding encoding)
    {
        byte[] buf = ArrayPool<byte>.Shared.Rent(_bufferSize);
        Span<byte> span = buf;

        char[]? decodedBuf = null;

        try
        {
            int decodedSize = encoding.GetMaxCharCount(span.Length);
            decodedBuf = ArrayPool<char>.Shared.Rent(decodedSize);
            Span<char> decodedSpan = decodedBuf;

            bool skipLF = false;
            bool hasWord = false;
            Decoder decoder = encoding.GetDecoder();

            int read = stream.Read(span);
            int decoded = decoder.GetChars(span.Slice(0, read), decodedSpan, read <= 0);

            while (decoded > 0)
            {
                Interlocked.Add(ref _bytes, (ulong)read);
                Interlocked.Add(ref _chars, (ulong)decoded);

                for (int i = 0; i < decoded; ++i)
                {
                    char ch = decodedSpan[i];

                    if (!char.IsWhiteSpace(ch))
                    {
                        hasWord = true;
                    }
                    else
                    {
                        if (hasWord)
                        {
                            Interlocked.Increment(ref _words);
                        }
                        hasWord = false;

                        if (ch == '\r')
                        {
                            Interlocked.Increment(ref _lines);
                            skipLF = true;
                        }
                        else if (ch == '\n')
                        {
                            if (!skipLF)
                            {
                                Interlocked.Increment(ref _lines);
                            }
                            skipLF = false;
                        }
                    }
                }

                read = stream.Read(span);
                decoded = decoder.GetChars(span.Slice(0, read), decodedSpan, read <= 0);
            }
        }
        finally
        {
            if (decodedBuf is not null)
            {
                ArrayPool<char>.Shared.Return(decodedBuf);
            }

            ArrayPool<byte>.Shared.Return(buf);
        }
    }
}
