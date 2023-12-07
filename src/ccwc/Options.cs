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

using CommandLine;

namespace ccwc;

class Options
{
    [Option('c', "bytes")]
    public bool CountBytes { get; }

    [Option('l', "lines")]
    public bool CountLines { get; }

    [Option('w', "words")]
    public bool CountWords { get; }

    [Option('m', "chars")]
    public bool CountCharacters { get; }

    [Value(0, MetaName = "FILE")]
    public string? FileName { get; }

    public Options(bool countBytes, bool countLines, bool countWords, bool countCharacters, string? fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(countBytes));

        CountBytes = countBytes;
        CountLines = countLines;
        CountWords = countWords;
        CountCharacters = countCharacters;
        FileName = fileName;
    }
}
