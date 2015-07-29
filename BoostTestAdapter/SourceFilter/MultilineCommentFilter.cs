using System.Text.RegularExpressions;
using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Filters any multiline comments from the source code
    /// </summary>
    public class MultilineCommentFilter : ISourceFilter
    {
        //regex used to extract the line breaks off a multiLine commented section
        private static readonly Regex lineBreakRegex = new Regex(@"(\r\n?|\n)", RegexOptions.Singleline | RegexOptions.Multiline);

        // (\r\n?|\n)
        //
        // Options: Case insensitive; Exact spacing; Dot matches line breaks; ^$ don't match at line breaks; Numbered capture
        //
        // Match the regex below and capture its match into backreference number 1 «(\r\n?|\n)»
        //    Match this alternative (attempting the next alternative only if this one fails) «\r\n?»
        //       Match the carriage return character «\r»
        //       Match the line feed character «\n?»
        //          Between zero and one times, as many times as possible, giving back as needed (greedy) «?»
        //    Or match this alternative (the entire group fails if this one fails to match) «\n»
        //       Match the line feed character «\n»

        private static readonly Regex multiLineCommentRegex = new Regex(@"(/\*(?:.+?)\*/)", RegexOptions.Singleline | RegexOptions.Multiline);
        // (/\*(?:.+?)\*/)
        //
        // Options: Case insensitive; Exact spacing; Dot matches line breaks; ^$ don't match at line breaks; Numbered capture
        //
        // Match the regex below and capture its match into backreference number 1 «(/\*(?:.+?)\*/)»
        //    Match the character “/” literally «/»
        //    Match the character “*” literally «\*»
        //    Match the regular expression below «(?:.+?)»
        //       Match any single character «.+?»
        //          Between one and unlimited times, as few times as possible, expanding as needed (lazy) «+?»
        //    Match the character “*” literally «\*»
        //    Match the character “/” literally «/»

        #region ISourceFilter

        /// <summary>
        /// Filters any multiline comments from the source code.
        /// </summary>
        /// <param name="cppSourceFile">CppSourceFile object containing the source file information</param>
        /// <param name="definesHandler">not used for this filter</param>
        public void Filter(CppSourceFile cppSourceFile, Defines definesHandler)
        {
            Utility.Code.Require(cppSourceFile, "cppSourceFile");
            cppSourceFile.SourceCode = multiLineCommentRegex.Replace(cppSourceFile.SourceCode, ComputeMultiLineCommentReplacement);
        }

        #endregion ISourceFilter

        /// <summary>
        /// Provides the replacement string for a multiline commented section. The replacement string will just contain line breaks.
        /// </summary>
        /// <param name="multiLineCommentMatch">comment section for which a replacement string needs to be provided</param>
        /// <returns>replacement string containing only the line breaks off the section to be replace.  It is important to note that line breaks of 
        /// unix types or dos types present in the source are preserved</returns>
        private static string ComputeMultiLineCommentReplacement(Match multiLineCommentMatch)
        {
            string replacementString = "";

            Match matchLineBreak = lineBreakRegex.Match(multiLineCommentMatch.Groups[0].ToString());
            while (matchLineBreak.Success)
            {
                replacementString = replacementString + matchLineBreak.Groups[0]; //line break types are preserved
                matchLineBreak = matchLineBreak.NextMatch();
            }

            return replacementString;
        }
    }
}