using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results.LogEntryTypes
{
    /// <summary>
    /// A LogEntry specification detailing an exception message.
    /// </summary>
    public class LogEntryException : LogEntry
    {
        #region Constructors

        public LogEntryException() :
            this(null)
        {
        }

        /// <summary>
        /// Constructor accepting a detail message of type string
        /// </summary>
        /// <param name="detail">Exception detail message</param>
        public LogEntryException(string detail) :
            this(detail, null)
        {
        }

        /// <summary>
        /// Constructor accepting an exception detail message and a SourceFileInfo object
        /// </summary>
        /// <param name="detail">detail message of type string</param>
        /// <param name="source">Source file information related to this log message. May be null.</param>
        public LogEntryException(string detail, SourceFileInfo source)
            : base(source)
        {
            this.Detail = detail;
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