namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Aggregates a C++ source file path and its content.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
    public class CppSourceFile
    {
        /// <summary>
        /// Source code content of the C++ source file.
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// C++ source file path.
        /// </summary>
        public string FileName { get; set; }
    }
}