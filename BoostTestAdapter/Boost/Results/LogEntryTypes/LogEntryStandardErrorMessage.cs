// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.Boost.Results.LogEntryTypes
{
    /// <summary>
    /// Log entry for a message emitted to standard error.
    /// </summary>
    public class LogEntryStandardErrorMessage : LogEntry
    {
        #region Constructors

        public LogEntryStandardErrorMessage(string detail)
        {
            this.Detail = detail;
        }

        #endregion Constructors

        #region object overrides

        public override string ToString()
        {
            return "Standard Error";
        }

        #endregion object overrides
    }
}