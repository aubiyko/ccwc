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

using FluentAssertions;
using Xunit;

namespace ccwc.Tests;

public sealed class UnitTests : IDisposable
{
    readonly Stream _file;

    public UnitTests()
    {
        var fileOptions = new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.ReadWrite, Options = FileOptions.SequentialScan };
        _file = new FileStream("test.txt", fileOptions);
    }

    [Fact]
    public void ByteCount_IsCorrect()
    {
        var counter = new Counter();
        counter.CountBytes = true;

        counter.CountFor(_file);

        counter.Bytes.Should().Be(342190ul);
    }

    [Fact]
    public void LineCount_IsCorrect()
    {
        var counter = new Counter();
        counter.CountNewLines = true;

        counter.CountFor(_file);

        counter.Lines.Should().Be(7145ul);
    }

    public void Dispose() => _file.Dispose();
}
