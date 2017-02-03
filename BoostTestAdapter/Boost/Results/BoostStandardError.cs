// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
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
        /// <param name="target">The destination result collection. Possibly used for result aggregation.</param>
        public BoostStandardError(IDictionary<string, TestResult> target)
            : base(target)
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