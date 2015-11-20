// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;

namespace BoostTestAdapter.Utility
{
    static class CommandLineArgExtensions
    {
        public static IEnumerable<string> Split(this string str,
                                            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }

        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;
            bool isEscaping = false;

            return commandLine.Split(c =>
            {
                if (c == '\\' && !isEscaping) { isEscaping = true; return false; }

                if (c == '\"' && !isEscaping)
                    inQuotes = !inQuotes;

                isEscaping = false;

                return !inQuotes && Char.IsWhiteSpace(c)/*c == ' '*/;
            })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"').Replace("\\\"", "\""))
                .Where(arg => !string.IsNullOrEmpty(arg));
        }
    }

    /// <summary>
    /// 2-tuple defining the FileName and Arguments of a command line string
    /// </summary>
    public class CommandLine
    {
        public CommandLine() :
            this(string.Empty, string.Empty)
        {
        }

        public CommandLine(string fileName, string args)
        {
            FileName = fileName;
            Arguments = args;
        }

        public CommandLine(string fileName, IEnumerable<string> arguments)
        {
            FileName = fileName;
            Arguments = "";
            
            if (arguments == null)
                return;

            foreach(string arg in arguments)
            {
                Arguments += (arg.Contains(' ') ? "\"" + arg + "\"" : arg) + " ";
            }
        }

        public string FileName { get; set; }
        public string Arguments { get; set; }

        public override string ToString()
        {
            return FileName + ' ' + Arguments;
        }

        public static CommandLine FromString(string cmdLine)
        {
            cmdLine = (cmdLine == null) ? string.Empty : cmdLine;

            var splitCommandLine = CommandLineArgExtensions.SplitCommandLine(cmdLine);
            return new CommandLine(
                splitCommandLine.FirstOrDefault(),
                splitCommandLine.Skip(1)
            );
        }
    }
}
