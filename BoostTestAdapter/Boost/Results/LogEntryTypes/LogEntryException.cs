// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results.LogEntryTypes
{
    /// <summary>
    /// A LogEntry specification detailing an exception message.
    /// </summary>
    public class LogEntryException : LogEntry
    {
        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LogEntryException()
        {
        }
        
        #endregion Constructors

        /// <summary>
        /// Last Checkpoint source information.
        /// </summary>
        public SourceFileInfo LastCheckpoint { get; set; }

        /// <summary>
        /// Checkpoint detail message.
        /// </summary>
        public string CheckpointDetail { get; set; }

        /// <summary>
        /// returns a string with the description of the class
        /// </summary>
        /// <returns>string having the description of the class</returns>
        public override string ToString()
        {
            return "Exception";
        }
    }
}