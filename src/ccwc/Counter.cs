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

    ulong _bytes;

    public ulong Bytes => _bytes;

    public Counter(int bufferSize = 1024) => _buffer = new byte[bufferSize];

    public void Reset() => _bytes = 0;

    public void CountFor(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        int read = stream.Read(_buffer);
        while (read > 0)
        {
            _bytes += (ulong)read;

            read = stream.Read(_buffer);
        }
    }
}
