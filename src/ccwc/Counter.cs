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

public class Counter
{
    readonly byte[] _buffer;

    bool _countBytes;

    public bool CountBytes
    {
        get => _countBytes;
        set => _countBytes = value;
    }

    ulong _bytes;

    public ulong Bytes => _bytes;

    bool _countNewLines;

    public bool CountNewLines
    {
        get => _countNewLines;
        set => _countNewLines = value;
    }

    ulong _lines;

    public ulong Lines => _lines;

    bool _countWords;

    public bool CountWords
    {
        get => _countWords;
        set => _countWords = value;
    }

    ulong _words;

    public ulong Words => _words;

    bool _countChars;

    public bool CountCharacters
    {
        get => _countChars;
        set => _countChars = value;
    }

    ulong _chars;

    public ulong Characters => _chars;

    public Counter(int bufferSize = 1024) => _buffer = new byte[bufferSize];

    public void Reset() => _bytes = _lines = _words = _chars = 0;

    public void CountFor(Stream stream) => CountFor(stream, Encoding.Default);

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

        int read = stream.Read(buf);
        while (read > 0)
        {
            _bytes += (ulong)read;

            if (_countNewLines)
            {
                _lines += (ulong)buf.Slice(0, read).Count((byte)'\n');
            }

            read = stream.Read(buf);
        }
    }

    void CountSlow(Stream stream, Encoding encoding)
    {
        Span<byte> buf = _buffer;
        int decodedSize = encoding.GetMaxCharCount(_buffer.Length);
        Span<char> decodedBuffer = new char[decodedSize];

        bool hasWord = false;
        Decoder decoder = encoding.GetDecoder();

        int read = stream.Read(buf);
        while (read > 0)
        {
            _bytes += (ulong)read;

            int decoded = decoder.GetChars(buf.Slice(0, read), decodedBuffer, false);
            _chars += (ulong)decoded;

            for (int i = 0; i < decoded; ++i)
            {
                char ch = decodedBuffer[i];

                if (ch == '\n')
                {
                    ++_lines;
                }
                else if (!char.IsWhiteSpace(ch))
                {
                    hasWord = true;
                }
                else if (hasWord)
                {
                    hasWord = false;
                    ++_words;
                }
            }

            read = stream.Read(buf);
        }

        decoder.GetCharCount(new ReadOnlySpan<byte>(), true);
    }
}
