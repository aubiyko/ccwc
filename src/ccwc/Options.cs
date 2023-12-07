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
using CommandLine.Text;

namespace ccwc;

/// <summary>
/// Command line options.
/// </summary>
/// <remarks>This class is thread-safe.</remarks>
class Options
{
    /// <summary>
    /// Name of the standard input.
    /// </summary>
    public const string StdIn = "-";

    /// <summary>
    /// Gets the value indicating whether to perform byte count.
    /// </summary>
    [Option('c', "bytes", HelpText = "Count bytes.")]
    public bool CountBytes { get; }

    /// <summary>
    /// Gets the value indicating whether to perform line count.
    /// </summary>
    [Option('l', "lines", HelpText = "Count lines.")]
    public bool CountLines { get; }

    /// <summary>
    /// Gets the value indicating whether to perform word count.
    /// </summary>
    [Option('w', "words", HelpText = "Count words.")]
    public bool CountWords { get; }

    /// <summary>
    /// Gets the value indicating whether to perform character count.
    /// </summary>
    [Option('m', "chars", HelpText = "Count characters.")]
    public bool CountCharacters { get; }

    /// <summary>
    /// Gets the collection of input files.
    /// </summary>
    [Value(0, Default = new[] { StdIn }, HelpText = "A whitespace separated sequence of input files.")]
    public IEnumerable<string> FileNames { get; }

    /// <summary>
    /// Gets the collection of usage examples.
    /// </summary>
    [Usage]
    public static IEnumerable<Example> Usage { get; } = new[]
    {
        new Example("Count bytes, lines and words from stdin", new Options(false, false, false, false, Array.Empty<string>())),
        new Example("Count words from file A", new Options(false, false, true, false, new[] { "A" }))
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Options"/> class.
    /// </summary>
    /// <param name="countBytes">A value indicating whether to perform byte count.</param>
    /// <param name="countLines">A value indicating whether to perform line count.</param>
    /// <param name="countWords">A value indicating whether to perform word count.</param>
    /// <param name="countCharacters">A value indicating whether to perform character count.</param>
    /// <param name="fileNames">A collection of input files.</param>
    public Options(bool countBytes, bool countLines, bool countWords, bool countCharacters, IEnumerable<string> fileNames)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(countBytes));

        CountBytes = countBytes;
        CountLines = countLines;
        CountWords = countWords;
        CountCharacters = countCharacters;
        FileNames = fileNames;
    }
}
