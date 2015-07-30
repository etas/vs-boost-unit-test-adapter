using System.Text.RegularExpressions;
using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Filters any quoted strings from the sourcecode. This filtering process is required so as to simplyfy the subsequent filtering operations
    /// </summary>
    public class QuotedStringsFilter : ISourceFilter
    {
        private static readonly Regex quotedStringsRegex = new Regex(@"(?<!#include\s{1,})""(.*?)(?<!\\)""", RegexOptions.Multiline);
        /*
        *
        * Options: Case sensitive; Exact spacing; Dot matches line breaks; ^$ don't match at line breaks; Numbered capture
        *
        * Assert that it is impossible to match the regex below backwards at this position (negative lookbehind) «(?<!#include\s{1,})»
        *    Match the character string “#include” literally (case sensitive) «#include»
        *    Match a single character that is a “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s{1,}»
        *       Between one and unlimited times, as many times as possible, giving back as needed (greedy) «{1,}»
        * Match the character “"” literally «"»
        * Match the regex below and capture its match into backreference number 1 «(.*?)»
        *    Match any single character «.*?»
        *       Between zero and unlimited times, as few times as possible, expanding as needed (lazy) «*?»
        * Assert that it is impossible to match the regex below backwards at this position (negative lookbehind) «(?<!\\)»
        *    Match the backslash character «\\»
        * Match the character “"” literally «"»
         */

        private static readonly Regex stringLiteralsRegex = new Regex(@"L?R""(.*)\((?:.*?)\)\k<1>""", RegexOptions.Singleline);
        /*
        * Options: Case sensitive; Exact spacing; Dot matches line breaks; ^$ don't match at line breaks; Numbered capture
        *
        * Match the character “L” literally (case sensitive) «L?»
        *    Between zero and one times, as many times as possible, giving back as needed (greedy) «?»
        * Match the character string “R"” literally (case sensitive) «R"»
        * Match the regex below and capture its match into backreference number 1 «(.*)»
        *    Match any single character «.*»
        *       Between zero and unlimited times, as many times as possible, giving back as needed (greedy) «*»
        * Match the character “(” literally «\(»
        * Match the regular expression below «(?:.*?)»
        *    Match any single character «.*?»
        *       Between zero and unlimited times, as few times as possible, expanding as needed (lazy) «*?»
        * Match the character “)” literally «\)»
        * Match the same text that was most recently matched by capturing group number 1 (case sensitive; fail if the group did not participate in the match so far) «\k<1>»
        * Match the character “"” literally «"»
         */

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

        #region ISourceFilter

        /// <summary>
        /// Applies the quoted strings filter action on the supplied sourceCode string
        /// </summary>
        /// <param name="cppSourceFile">CppSourceFile object containing the source file information</param>
        /// <param name="definesHandler">not used for this filter</param>
        public void Filter(CppSourceFile cppSourceFile, Defines definesHandler)
        {
            Utility.Code.Require(cppSourceFile, "cppSourceFile");

            /*
             * It is important not to the change order of the filters.
             */
            cppSourceFile.SourceCode = stringLiteralsRegex.Replace(cppSourceFile.SourceCode, new MatchEvaluator(ComputeReplacement));

            cppSourceFile.SourceCode = quotedStringsRegex.Replace(cppSourceFile.SourceCode, "");
        }

        #endregion ISourceFilter

        /// <summary>
        ///  The string literal syntax can span over multiple lines so when generating the replacement string we need to make sure that the number of
        ///  line breaks and their types are preserved
        /// </summary>
        /// <param name="m">section of the source code to be replaced</param>
        /// <returns>the replacement string</returns>
        private string ComputeReplacement(Match m)
        {
            string replacementString = "";

            Match matchLineBreak = lineBreakRegex.Match(m.Value);
            while (matchLineBreak.Success)
            {
                replacementString = replacementString + matchLineBreak.Groups[0]; //line break types are preserved
                matchLineBreak = matchLineBreak.NextMatch();
            }

            return replacementString;
        }
    }
}