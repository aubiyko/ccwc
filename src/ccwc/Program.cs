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

using System.Globalization;
using System.Security;
using System.Text;
using ccwc;
using CommandLine;
using CommandLine.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var parser = new Parser(parserSettings => parserSettings.HelpWriter = null);
ParserResult<Options> result = parser.ParseArguments<Options>(args);
return result.MapResult(Run, _ => DisplayHelp(result));

static int Run(Options cliOptions)
{
    var results = new List<(string, Counter)>(cliOptions.FileNames.Count());

    try
    {
        int codePage = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
        var encoding = Encoding.GetEncoding(codePage);

        ulong totalLines = 0;
        ulong totalWords = 0;
        ulong totalChars = 0;
        ulong totalBytes = 0;

        foreach (string fileName in cliOptions.FileNames)
        {
            Counter counter = Count(cliOptions, fileName, encoding);
            results.Add((fileName, counter));

            totalLines += counter.Lines;
            totalWords += counter.Words;
            totalChars += counter.Characters;
            totalBytes += counter.Bytes;
        }

        int linesLen = GetDigitsNumber(totalLines);
        int wordsLen = GetDigitsNumber(totalWords);
        int charsLen = GetDigitsNumber(totalChars);
        int bytesLen = GetDigitsNumber(totalBytes);

        string linesFmt = "{0," + linesLen + "} ";
        string wordsFmt = "{0," + wordsLen + "} ";
        string charsFmt = "{0," + charsLen + "} ";
        string bytesFmt = "{0," + bytesLen + "} ";

        foreach ((string fileName, Counter counter) in results)
        {
            if (counter.CountNewLines)
            {
                Console.Write(linesFmt, counter.Lines);
            }
            if (counter.CountWords)
            {
                Console.Write(wordsFmt, counter.Words);
            }
            if (counter.CountCharacters)
            {
                Console.Write(charsFmt, counter.Characters);
            }
            if (counter.CountBytes)
            {
                Console.Write(bytesFmt, counter.Bytes);
            }

            string simpleFileName = Path.GetFileName(fileName);
            Console.WriteLine(simpleFileName);
        }

        if (results.Count > 1)
        {
            (_, Counter counter) = results[0];
            if (counter.CountNewLines)
            {
                Console.Write(linesFmt, totalLines);
            }
            if (counter.CountWords)
            {
                Console.Write(wordsFmt, totalWords);
            }
            if (counter.CountCharacters)
            {
                Console.Write(charsFmt, totalChars);
            }
            if (counter.CountBytes)
            {
                Console.Write(bytesFmt, totalBytes);
            }

            Console.WriteLine("total");
        }
    }
    catch (SystemException ex) when (ex is ArgumentException
        || ex is NotSupportedException
        || ex is IOException
        || ex is SecurityException
        || ex is UnauthorizedAccessException)
    {
        Console.Error.WriteLine(ex.Message);
        return -1;
    }

    return 0;
}

static Counter Count(Options cliOptions, string fileName, Encoding encoding)
{
    var counter = new Counter();

    if (!cliOptions.CountBytes && !cliOptions.CountLines && !cliOptions.CountWords && !cliOptions.CountCharacters)
    {
        counter.CountBytes = counter.CountNewLines = counter.CountWords = true;
    }
    else
    {
        counter.CountBytes = cliOptions.CountBytes;
        counter.CountNewLines = cliOptions.CountLines;
        counter.CountWords = cliOptions.CountWords;
        counter.CountCharacters = cliOptions.CountCharacters;
    }

    Stream? input = null;

    try
    {
        if (fileName == Options.StdIn)
        {
            input = Console.OpenStandardInput();
            encoding = Console.InputEncoding;
        }
        else
        {
            var fileOptions = new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.ReadWrite, Options = FileOptions.SequentialScan };
            input = new FileStream(fileName, fileOptions);
        }
        counter.CountFor(input, encoding);

        return counter;
    }
    finally
    {
        input?.Dispose();
    }
}

static int GetDigitsNumber(ulong num)
{
    if (num == 0)
    {
        return 1;
    }

    return (int)Math.Floor(Math.Log10(num)) + 1;
}

static int DisplayHelp(ParserResult<Options> result)
{
    var helpText = HelpText.AutoBuild(result, h =>
    {
        h.AdditionalNewLineAfterOption = false;
        h.Heading = @"ccwc Copyright (C) 2023 aubiyko. All rights reserved.
This program comes with ABSOLUTELY NO WARRANTY.
This is free software, and you are welcome to redistribute it under certain
conditions; see the file LICENSE.md for details.";
        h.Copyright = "";
        return h;
    });

    Console.WriteLine(helpText);
    return result.Errors.IsHelp() ? 0 : -1;
}
