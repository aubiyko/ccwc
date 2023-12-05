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

    public Counter(int bufferSize = 1024) => _buffer = new byte[bufferSize];

    public void Reset() => _bytes = _lines = 0;

    public void CountFor(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        ThrowIfNoMode();

        int read = stream.Read(_buffer);
        while (read > 0)
        {
            _bytes += (ulong)read;

            if (_countNewLines)
            {
                CountLines(read);
            }

            read = stream.Read(_buffer);
        }
    }

    void ThrowIfNoMode()
    {
        if (!_countBytes && !_countNewLines)
        {
            throw new InvalidOperationException();
        }
    }

    void CountLines(int len)
    {
        ReadOnlySpan<byte> span = _buffer;
        _lines += (ulong)span.Slice(0, len).Count((byte)'\n');
    }
}
