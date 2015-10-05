// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results.LogEntryTypes
{
    /// <summary>
    /// Base class for Log entries
    /// </summary>
    public abstract class LogEntry
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected LogEntry()
        {
        }

        /// <summary>
        /// Constructor accepting a SourceFileInfo
        /// </summary>
        /// <param name="source">Source file information related to this log message. May be null.</param>
        protected LogEntry(SourceFileInfo source) :
            this()
        {
            this.Source = source;
        }

        #endregion Constructors

        /// <summary>
        /// Source file information related to this log message. May be null.
        /// </summary>
        public SourceFileInfo Source { get; set; }

        /// <summary>
        /// returns a string with the description of the class
        /// </summary>
        /// <returns>string having the description of the class</returns>
        public override string ToString()
        {
            return "Base";
        }

        /// <summary>
        /// Log detail message.
        /// </summary>
        public string Detail { get; set; }
    }
}