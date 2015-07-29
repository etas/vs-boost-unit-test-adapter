using System.Text.RegularExpressions;
using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// ISourceFilter implementation. Filters single line comments.
    /// </summary>
    public class SingleLineCommentFilter : ISourceFilter
    {
        private static readonly Regex singleLineCommentRegex = new Regex(@"(?://(?:.*))", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        #region ISourceFilter

        /// <summary>
        /// Filters any single line comments from the source code
        /// </summary>
        /// <param name="cppSourceFile">CppSourceFile object containing the source file information</param>
        /// <param name="definesHandler">not used for this filter</param>
        public void Filter(CppSourceFile cppSourceFile, Defines definesHandler)
        {
            Utility.Code.Require(cppSourceFile, "cppSourceFile");

            cppSourceFile.SourceCode = singleLineCommentRegex.Replace(cppSourceFile.SourceCode, "");
        }

        #endregion ISourceFilter
    }
}