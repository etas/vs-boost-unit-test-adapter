using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// An ISourceFilter filters out redundant information from source code.
    /// </summary>
    public interface ISourceFilter
    {
        /// <summary>
        /// Filters the provided C++ source file's source code.
        /// </summary>
        /// <param name="cppSourceFile">The C++ source file to filter</param>
        /// <param name="definesHandler">C++ preprocessor definitions</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "cpp")]
        void Filter(CppSourceFile cppSourceFile, Defines definesHandler);
    }
}