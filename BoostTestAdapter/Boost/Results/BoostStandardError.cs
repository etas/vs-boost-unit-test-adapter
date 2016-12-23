// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using BoostTestAdapter.Boost.Results.LogEntryTypes;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Standard Error as emitted by Boost Test executables
    /// </summary>
    public class BoostStandardError : BoostConsoleOutputBase
    {
        /// <summary>
        /// Constructor accepting a path to the external file
        /// </summary>
        /// <param name="path">The path to an external file. File will be opened on construction.</param>
        public BoostStandardError(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Constructor accepting a stream to the file contents
        /// </summary>
        /// <param name="stream">The file content stream.</param>
        public BoostStandardError(Stream stream)
            : base(stream)
        {
        }
        
        #region BoostConsoleOutputBase

        protected override LogEntry CreateLogEntry(string message)
        {
            return new LogEntryStandardErrorMessage()
            {
                Detail = message
            };
        }

        #endregion BoostConsoleOutputBase        
    }
}