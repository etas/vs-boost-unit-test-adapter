// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (IsQuoted(input, quote))
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }

        /// <summary>
        /// States if the provided string is quoted using the provided quotation marks
        /// </summary>
        /// <param name="input">The input string to test</param>
        /// <param name="mark">The quotation mark to test for</param>
        /// <returns>true if the input string is within quotation marks; false otherwise</returns>
        internal static bool IsQuoted(string input, char mark = '"')
        {
            return (input.Length >= 2) && (input[0] == mark) && (input[input.Length - 1] == mark);
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
                if ((c == '\\') && (!isEscaping))
                {
                    isEscaping = true;
                    return false;
                }

                if ((c == '\"') && (!isEscaping))
                {
                    inQuotes = !inQuotes;
                }

                isEscaping = false;

                return !inQuotes && char.IsWhiteSpace(c);
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
        public CommandLine(string fileName, IEnumerable<string> arguments) :
            this(fileName, JoinArguments(arguments))
        {
        }

        public string FileName { get; set; }
        public string Arguments { get; set; }

        public override string ToString()
        {
            return NormalizePath(FileName) + ' ' + Arguments;
        }

        /// <summary>
        /// Generates a CommandLine instance from the provided string
        /// </summary>
        /// <param name="cmdLine">Command line string</param>
        /// <returns>The command line instance parsed from cmdLine</returns>
        public static CommandLine FromString(string cmdLine)
        {
            cmdLine = cmdLine ?? string.Empty;

            var splitCommandLine = CommandLineArgExtensions.SplitCommandLine(cmdLine);
            return new CommandLine(
                splitCommandLine.FirstOrDefault(),
                splitCommandLine.Skip(1)
            );
        }

        /// <summary>
        /// Concatenates a collection of strings as a valid command-line argument set
        /// </summary>
        /// <param name="arguments">The arguments to serialize</param>
        /// <returns>string representing the concatenation set of all arguments</returns>
        private static string JoinArguments(IEnumerable<string> arguments)
        {
            if (arguments == null)
            {
                return string.Empty;
            }

            var quotedArgs = arguments.Select(arg => arg.Contains(' ') ? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", arg) : arg);
            return string.Join(" ", quotedArgs);
        }

        /// <summary>
        /// Normalizes the file path and adds quotes should the path not be quoted already
        /// </summary>
        /// <param name="filePath">The path to notmalized</param>
        /// <returns>A normalized file path</returns>
        private static string NormalizePath(string filePath)
        {
            var path = filePath?.Trim();

            // Invalid input, return immediately
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            const char mark = '"';

            // If the path is already quoted, leave as is
            if (CommandLineArgExtensions.IsQuoted(path, mark))
            {
                return path;
            }

            return path.Contains(' ') ? (mark + path + mark) : path;
        }
    }
}
