// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Text;
using System.Text.RegularExpressions;
using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Filters any quoted strings from the sourcecode. This filtering process is required so as to simplyfy the subsequent filtering operations
    /// </summary>
    public class QuotedStringsFilter : ISourceFilter
    {
        // Reference: http://stackoverflow.com/questions/2039795/regular-expression-for-a-string-literal-in-flex-lex
        private static readonly Regex stringLiteralsRegex = new Regex(@"(L?R""(.*?)\((?:.*?)\)\k<2>"")|(?:""(?:\\.|[^""])*"")", RegexOptions.Singleline);

        //Options: Case sensitive; Exact spacing; Dot matches line breaks; ^$ don't match at line breaks; Numbered capture
        //
        //Match this alternative (attempting the next alternative only if this one fails) «(L?R"(.*)\((?:.*?)\)\k<2>")»
        //   Match the regex below and capture its match into backreference number 1 «(L?R"(.*)\((?:.*?)\)\k<2>")»
        //      Match the character “L” literally (case sensitive) «L?»
        //         Between zero and one times, as many times as possible, giving back as needed (greedy) «?»
        //      Match the character string “R"” literally (case sensitive) «R"»
        //      Match the regex below and capture its match into backreference number 2 «(.*)»
        //         Match any single character «.*»
        //            Between zero and unlimited times, as many times as possible, giving back as needed (greedy) «*»
        //      Match the character “(” literally «\(»
        //      Match the regular expression below «(?:.*?)»
        //         Match any single character «.*?»
        //            Between zero and unlimited times, as few times as possible, expanding as needed (lazy) «*?»
        //      Match the character “)” literally «\)»
        //      Match the same text that was most recently matched by capturing group number 2 (case sensitive; fail if the group did not participate in the match so far) «\k<2>»
        //      Match the character “"” literally «"»
        //Or match this alternative (the entire match attempt fails if this one fails to match) «(?:"(?:\\.|[^"])*")»
        //   Match the regular expression below «(?:"(?:\\.|[^"])*")»
        //      Match the character “"” literally «"»
        //      Match the regular expression below «(?:\\.|[^"])*»
        //         Between zero and unlimited times, as many times as possible, giving back as needed (greedy) «*»
        //         Match this alternative (attempting the next alternative only if this one fails) «\\.»
        //            Match the backslash character «\\»
        //            Match any single character «.»
        //         Or match this alternative (the entire group fails if this one fails to match) «[^"]»
        //            Match any character that is NOT a “"” «[^"]»
        //      Match the character “"” literally «"»

        private static readonly Regex lineBreakRegex = new Regex(@"\r\n?|\n", RegexOptions.Singleline | RegexOptions.Multiline);
        
        //Options: Case insensitive; Exact spacing; Dot matches line breaks; ^$ don't match at line breaks; Numbered capture
        //
        //Match the regex below and capture its match into backreference number 1 «(\r\n?|\n)»
        //   Match this alternative (attempting the next alternative only if this one fails) «\r\n?»
        //      Match the carriage return character «\r»
        //      Match the line feed character «\n?»
        //         Between zero and one times, as many times as possible, giving back as needed (greedy) «?»
        //   Or match this alternative (the entire group fails if this one fails to match) «\n»
        //      Match the line feed character «\n»

        #region ISourceFilter

        /// <summary>
        /// Applies the quoted strings filter action on the supplied sourceCode string
        /// </summary>
        /// <param name="cppSourceFile">CppSourceFile object containing the source file information</param>
        /// <param name="definesHandler">not used for this filter</param>
        public void Filter(CppSourceFile cppSourceFile, Defines definesHandler)
        {
            Utility.Code.Require(cppSourceFile, "cppSourceFile");

            cppSourceFile.SourceCode = stringLiteralsRegex.Replace(cppSourceFile.SourceCode, ComputeReplacement);
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
            if (m.Groups.Count > 1 && m.Groups[1].Success)
            {
                StringBuilder replacementString = new StringBuilder();

                Match matchLineBreak = lineBreakRegex.Match(m.Groups[1].Value);
                while (matchLineBreak.Success)
                {
                    replacementString.Append(matchLineBreak.Value); //line break types are preserved
                    matchLineBreak = matchLineBreak.NextMatch();
                }

                return replacementString.ToString();
            }

            return string.Empty;
        }
    }
}