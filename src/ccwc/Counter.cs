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

using System.Text;

namespace ccwc;

/// <summary>
/// Bytes or text cumulative statistics.
/// </summary>
public class Counter
{
    readonly byte[] _buffer;

    bool _countBytes;

    /// <summary>
    /// Gets or sets the value indicating whether to perform byte count.
    /// </summary>
    public bool CountBytes
    {
        get => _countBytes;
        set => _countBytes = value;
    }

    ulong _bytes;

    /// <summary>
    /// Gets the amount of bytes.
    /// </summary>
    public ulong Bytes => _bytes;

    bool _countNewLines;

    /// <summary>
    /// Gets or sets the value indicating whether to perform line count.
    /// </summary>
    /// <remarks>See <see cref="Lines"/> for line definition.</remarks>
    public bool CountNewLines
    {
        get => _countNewLines;
        set => _countNewLines = value;
    }

    ulong _lines;

    /// <summary>
    /// Gets the amount of lines.
    /// A line is defined as a sequence of bytes ending on CR, LF or CR & LF.
    /// </summary>
    public ulong Lines => _lines;

    bool _countWords;

    /// <summary>
    /// Gets or sets the value indicating whether to perform word count.
    /// </summary>
    /// <remarks>See <see cref="Words"/> for word definition.</remarks>
    public bool CountWords
    {
        get => _countWords;
        set => _countWords = value;
    }

    ulong _words;

    /// <summary>
    /// Gets the amount of words.
    /// A word is the sequence of non-whitespace bytes.
    /// </summary>
    public ulong Words => _words;

    bool _countChars;

    /// <summary>
    /// Gets or sets the value indicating whether to perform character count.
    /// </summary>
    public bool CountCharacters
    {
        get => _countChars;
        set => _countChars = value;
    }

    ulong _chars;

    /// <summary>
    /// Gets the amount of characters.
    /// </summary>
    public ulong Characters => _chars;

    /// <summary>
    /// Initializes a new instance of the <see cref="Counter"/> class.
    /// </summary>
    /// <param name="bufferSize">A read buffer size, in bytes.</param>
    public Counter(int bufferSize = 1024) => _buffer = new byte[bufferSize];

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

        if (!_countWords && !_countChars)
        {
            CountFast(stream);
        }
        else
        {
            CountSlow(stream, encoding);
        }
    }

    void ThrowIfNoMode()
    {
        if (!_countBytes && !_countNewLines && !_countWords && !_countChars)
        {
            throw new InvalidOperationException();
        }
    }

    void CountFast(Stream stream)
    {
        Span<byte> buf = _buffer;

        bool skipLF = false;

        int read = stream.Read(buf);
        while (read > 0)
        {
            _bytes += (ulong)read;

            if (_countNewLines)
            {
                skipLF = CountLines(buf.Slice(0, read), skipLF);
            }

            read = stream.Read(buf);
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
                ++_lines;
                skipLF = true;
            }
            else if (c == LF)
            {
                if (!skipLF)
                {
                    ++_lines;
                }
                skipLF = false;
            }
        }

        return skipLF;
    }

    void CountSlow(Stream stream, Encoding encoding)
    {
        Span<byte> buf = _buffer;
        int decodedSize = encoding.GetMaxCharCount(_buffer.Length);
        Span<char> decodedBuffer = new char[decodedSize];

        bool skipLF = false;
        bool hasWord = false;
        Decoder decoder = encoding.GetDecoder();

        int read = stream.Read(buf);
        int decoded = decoder.GetChars(buf.Slice(0, read), decodedBuffer, read <= 0);

        while (decoded > 0)
        {
            _bytes += (ulong)read;
            _chars += (ulong)decoded;

            for (int i = 0; i < decoded; ++i)
            {
                char ch = decodedBuffer[i];

                if (!char.IsWhiteSpace(ch))
                {
                    hasWord = true;
                }
                else
                {
                    if (hasWord)
                    {
                        ++_words;
                    }
                    hasWord = false;

                    if (ch == '\r')
                    {
                        ++_lines;
                        skipLF = true;
                    }
                    else if (ch == '\n')
                    {
                        if (!skipLF)
                        {
                            ++_lines;
                        }
                        skipLF = false;
                    }
                }
            }

            read = stream.Read(buf);
            decoded = decoder.GetChars(buf.Slice(0, read), decodedBuffer, read <= 0);
        }
    }
}
