namespace BoostTestAdapter.Boost.Results.LogEntryTypes
{
    /// <summary>
    /// Log entry for a message emitted to standard output.
    /// </summary>
    public class LogEntryStandardOutputMessage : LogEntry
    {
        #region Constructors

        public LogEntryStandardOutputMessage(string detail)
        {
            this.Detail = detail;
        }

        #endregion Constructors

        #region object overrides

        public override string ToString()
        {
            return "Standard Output";
        }

        #endregion object overrides
    }
}