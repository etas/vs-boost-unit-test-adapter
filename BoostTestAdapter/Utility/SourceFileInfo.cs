using System.IO;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Identifies a source file and a respective line of interest.
    /// </summary>
    public class SourceFileInfo
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The source file path.</param>
        public SourceFileInfo(string file) :
            this(file, -1)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The source file path.</param>
        /// <param name="lineNumber">The associated line number of interest or -1 if not available.</param>
        public SourceFileInfo(string file, int lineNumber)
        {
            this.File = file;
            this.LineNumber = lineNumber;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Source file path.
        /// </summary>
        public string File { get; private set; }

        /// <summary>
        /// Line number within File. Defaults to -1 meaning no line number information is available.
        /// </summary>
        public int LineNumber { get; set; }

        #endregion Properties

        #region object overrides

        public override string ToString()
        {
            string file = Path.GetFileName(this.File);
            if ((string.IsNullOrEmpty(file)) && (this.LineNumber > -1))
            {
                file = "unknown location";
            }

            return file + ((this.LineNumber > -1) ? (" line " + this.LineNumber) : string.Empty);
        }

        #endregion object overrides
    }
}