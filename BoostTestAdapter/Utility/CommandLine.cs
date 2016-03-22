// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Utility methods for CommandLine
    /// </summary>
    static class CommandLineArgExtensions
    {
        /// <summary>
        /// Splits a string based on the provided function
        /// </summary>
        /// <param name="str">The string to split</param>
        /// <param name="controller">The function which determines when a split should occur</param>
        /// <returns>An enumeration of string splits</returns>
        public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
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

        /// <summary>
        /// Removes the character 'quote' from the beginning and end of 'input'
        /// </summary>
        /// <param name="input">The input to un-quote</param>
        /// <param name="quote">The quote character to remove</param>
        /// <returns>'input' with the beginning and end 'quote' removed</returns>
        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }

        /// <summary>
        /// Safely splits a command line string into its argument components.
        /// </summary>
        /// <param name="commandLine">The command line string to split</param>
        /// <returns>A series of substrings, each of which represent a single command line argument component</returns>
        /// <remarks>Properly handles string/character escaping</remarks>
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
        /// <summary>
        /// Default constructor
        /// </summary>
        public CommandLine() :
            this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The file component of the command line</param>
        /// <param name="args">The arguments component of the command line</param>
        public CommandLine(string fileName, string args)
        {
            FileName = fileName;
            Arguments = args;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The file component of the command line</param>
        /// <param name="arguments">The arguments of the command line</param>
        /// <remarks>Handles quoting in case of spaces</remarks>
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

        /// <summary>
        /// Generates a CommandLine instance from the provided string
        /// </summary>
        /// <param name="cmdLine">Command line string</param>
        /// <returns>The command line instance parsed from cmdLine</returns>
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
