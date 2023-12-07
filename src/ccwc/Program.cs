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
        Encoding encoding;

        string? fileName = cliOptions.FileName;
        if (string.IsNullOrEmpty(fileName) || fileName == "-")
        {
            input = Console.OpenStandardInput();
            encoding = Console.InputEncoding;
        }
        else
        {
            var fileOptions = new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.ReadWrite, Options = FileOptions.SequentialScan };
            input = new FileStream(fileName, fileOptions);

            int codePage = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
            encoding = Encoding.GetEncoding(codePage);
        }
        counter.CountFor(input, encoding);
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
    finally
    {
        input?.Dispose();
    }

    const string Format = " {0}";
    string currentFormat = "{0}";

    if (counter.CountNewLines)
    {
        Console.Write(currentFormat, counter.Lines);
        currentFormat = Format;
    }
    if (counter.CountWords)
    {
        Console.Write(currentFormat, counter.Words);
        currentFormat = Format;
    }
    if (counter.CountCharacters)
    {
        Console.Write(currentFormat, counter.Characters);
        currentFormat = Format;
    }
    if (counter.CountBytes)
    {
        Console.Write(currentFormat, counter.Bytes);
    }

    return 0;
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
