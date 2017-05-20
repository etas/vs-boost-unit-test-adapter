// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Globalization;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Identifies a source file and a respective line of interest.
    /// </summary>
    public class SourceFileInfo
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The source file path.</param>
        public SourceFileInfo(string file) :
            this(file, -1)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The source file path.</param>
        /// <param name="lineNumber">The associated line number of interest or -1 if not available.</param>
        public SourceFileInfo(string file, int lineNumber)
        {
            this.File = file;
            this.LineNumber = lineNumber;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Source file path.
        /// </summary>
        public string File { get; private set; }

        /// <summary>
        /// Line number within File. Defaults to -1 meaning no line number information is available.
        /// </summary>
        public int LineNumber { get; set; }

        #endregion Properties

        #region object overrides

        public override string ToString()
        {
            string file = Path.GetFileName(this.File);
            if ((string.IsNullOrEmpty(file)) && (this.LineNumber > -1))
            {
                file = "unknown location";
            }

            return file + ((this.LineNumber > -1) ? ("(" + this.LineNumber + ")") : string.Empty);
        }

        /// <summary>
        /// Parses source file information
        /// </summary>
        /// <param name="value">The string representation of the source file information</param>
        /// <returns>The SourceFileInfo structure or null if the value cannot be parsed correctly</returns>
        public static SourceFileInfo Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            int bracketIndex = value.IndexOf('(');

            string filename = (bracketIndex > 0) ? value.Substring(0, bracketIndex) : value;
            string linenumber = (bracketIndex > 0) ? value.Substring(bracketIndex + 1, (value.Length - filename.Length - 2)) : string.Empty;

            int line = (string.IsNullOrEmpty(linenumber) ? -1 : int.Parse(linenumber, CultureInfo.InvariantCulture));

            return new SourceFileInfo(filename, line);
        }

        #endregion object overrides
    }
}