// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Collections.Generic;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results.LogEntryTypes
{
    /// <summary>
    /// 'Error' log entry
    /// </summary>
    public class LogEntryError : LogEntry
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogEntryError()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Constructor accepting a detail message
        /// </summary>
        /// <param name="detail">detail message of type string</param>
        public LogEntryError(string detail)
            : this(detail, null)
        {
        }

        /// <summary>
        /// Constructor accepting a detail message and a SourceFileInfo object
        /// </summary>
        /// <param name="detail">detail message of type string</param>
        /// <param name="source">Source file information related to this log message. May be null.</param>
        public LogEntryError(string detail, SourceFileInfo source)
            : this(detail, source, Enumerable.Empty<string>())
        {
        }


        /// <summary>
        /// Constructor accepting a detail message, a SourceFileInfo object and error context frames
        /// </summary>
        /// <param name="detail">detail message of type string</param>
        /// <param name="source">Source file information related to this log message. May be null.</param>
        /// <param name="contextFrames">Error context frame information related to this error message. May be empty.</param>
        public LogEntryError(string detail, SourceFileInfo source, IEnumerable<string> contextFrames)
            : base(source)
        {
            this.Detail = detail;
            this.ContextFrames = contextFrames;
        }

        #endregion Constructors

        /// <summary>
        /// Context frame information.
        /// </summary>
        public IEnumerable<string> ContextFrames { get; set; }
        
        /// <summary>
        /// returns a string with the description of the class
        /// </summary>
        /// <returns>string having the description of the class</returns>
        public override string ToString()
        {
            return "Error";
        }
    }
}