// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Collections.Generic;

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
            Detail = string.Empty;
            Source = null;
            ContextFrames = Enumerable.Empty<string>();
        }
        
        #endregion

        /// <summary>
        /// Source file information related to this log message. May be null.
        /// </summary>
        public SourceFileInfo Source { get; set; }

        /// <summary>
        /// Log detail message.
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// Context frame information.
        /// </summary>
        public IEnumerable<string> ContextFrames { get; set; }

        /// <summary>
        /// Constructs a LogEntry and populates the main components
        /// </summary>
        /// <typeparam name="T">A derived LogEntry class type</typeparam>
        /// <param name="detail">The detail message. 'null' to use the default.</param>
        /// <param name="source">The source file information assocaited to the log entry. 'null' to use the default.</param>
        /// <param name="context">The log entry context. 'null' to use the default.</param>
        /// <returns>A new LogEntry-derived instance populated accordingly</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static T MakeLogEntry<T>(string detail = null, SourceFileInfo source = null, IEnumerable<string> context = null) where T : LogEntry, new()
        {
            T entry = new T();

            if (detail != null)
            {
                entry.Detail = detail;
            }

            if (source != null)
            {
                entry.Source = source;
            }

            if (context != null)
            {
                entry.ContextFrames = context;
            }

            return entry;
        }

        #region Object overrides

        /// <summary>
        /// returns a string with the description of the class
        /// </summary>
        /// <returns>string having the description of the class</returns>
        public override string ToString()
        {
            return "Base";
        }

        #endregion
    }
}