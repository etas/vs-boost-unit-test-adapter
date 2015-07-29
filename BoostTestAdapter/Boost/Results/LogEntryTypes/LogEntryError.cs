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
        /// Constructor accepting a detail message
        /// </summary>
        /// <param name="detail">detail message of type string</param>
        public LogEntryError(string detail)
        {
            this.Detail = detail;
        }

        /// <summary>
        /// Constructor accepting a detail message and a SourceFileInfo object
        /// </summary>
        /// <param name="detail">detail message of type string</param>
        /// <param name="source">Source file information related to this log message. May be null.</param>
        public LogEntryError(string detail, SourceFileInfo source)
            : base(source)
        {
            this.Detail = detail;
        }

        #endregion Constructors

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