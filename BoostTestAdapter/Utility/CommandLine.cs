﻿using System;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// 2-tuple defining the FileName and Arguments of a command line string
    /// </summary>
    public class CommandLine
    {
        public CommandLine() :
            this(string.Empty, string.Empty)
        {
        }

        public CommandLine(string fileName, string arguments)
        {
            FileName = fileName;
            Arguments = arguments;
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
            int index = cmdLine.IndexOf(' ');

            return new CommandLine
            {
                FileName = cmdLine.Substring(0, Math.Max(0, index)),
                Arguments = cmdLine.Substring(index + 1)
            };
        }
    }
}
