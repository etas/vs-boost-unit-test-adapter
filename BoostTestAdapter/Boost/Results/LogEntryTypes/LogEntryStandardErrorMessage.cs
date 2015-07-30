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