using System.IO;
using System.Text.RegularExpressions;
using BoostTestAdapter.Boost.Results.LogEntryTypes;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Standard Output as emitted by Boost Test executables
    /// </summary>
    public class BoostStandardOutput : BoostTestResultOutputBase
    {
        #region Constructors

        /// <summary>
        /// Constructor accepting a path to the external file
        /// </summary>
        /// <param name="path">The path to an external file. File will be opened on construction.</param>
        public BoostStandardOutput(string path)
            : base(path)
        {
            this.FailTestOnMemoryLeak = false;
        }

        /// <summary>
        /// Constructor accepting a stream to the file contents
        /// </summary>
        /// <param name="stream">The file content stream.</param>
        public BoostStandardOutput(Stream stream)
            : base(stream)
        {
            this.FailTestOnMemoryLeak = false;
        }

        #endregion Constructors

        #region Properties

        public bool FailTestOnMemoryLeak { get; set; }

        #endregion Properties

        #region IBoostOutputParser

        /// <summary>
        /// Processes the standard output and populates the relevant test result data of the referenced collection
        /// </summary>
        /// <param name="collection">test result collection where the leak information data will be inserted at</param>
        public override void Parse(TestResultCollection collection)
        {
            string strConsoleOutput = StreamToString(this.InputStream);

            //the below regex is intended to only to "detect" if any memory leaks are present. Note that any console output printed by the test generally appears before the memory leaks dump.
            Regex regexObj = new Regex(@"Detected\smemory\sleaks!\nDumping objects\s->\n(.*)Object dump complete.", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
            Match outputMatch = regexObj.Match(strConsoleOutput);

            //leak has been detected
            if (outputMatch.Success)
            {
                RegisterMemoryLeak(outputMatch.Groups[1].Value, collection);
            }

            // Extract non-memory leak output
            string output = strConsoleOutput.Substring(0, ((outputMatch.Success) ? outputMatch.Index : strConsoleOutput.Length));
            RegisterStandardOutputMessage(output, collection);
        }

        #endregion IBoostOutputParser

        private void RegisterMemoryLeak(string leakInformation, TestResultCollection collection)
        {
            foreach (TestResult result in collection)
            {
                if (this.FailTestOnMemoryLeak)
                {
                    result.Result = TestResultType.Failed;
                }

                Regex regexLeakInformation = new Regex(@"(?:([\\:\w\rA-z.]*?)([\w\d.]*)\((\d{1,})\)\s:\s)?\{(\d{1,})\}[\w\s\d]*,\s(\d{1,})[\s\w.]*\n(.*?)(?=$|(?:[\\\w.:]*\(\d{1,}\)\s:\s)?\{\d{1,}\d)", RegexOptions.IgnoreCase | RegexOptions.Singleline);   //the old one wasRegex regexLeakInformation = new Regex(@"^(.*\\)(.*?)\((\d{1,})\).*?{(\d{1,})}.*?(\d{1,})\sbyte", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                /*

                 The same regex works for when the complete file path along with the line number are reported in the console output such as in the below sample output

                 d:\hwa\dev\svn\boostunittestadapterdev\branches\tempbugfixing\sample\boostunittest\boostunittest2\adapterbugs.cpp(58) : {869} normal block at 0x00A88A58, 4 bytes long.
                    Data: <    > CD CD CD CD
                 d:\hwa\dev\svn\boostunittestadapterdev\branches\tempbugfixing\sample\boostunittest\boostunittest2\adapterbugs.cpp(55) : {868} normal block at 0x00A88788, 4 bytes long.
                    Data: <    > F5 01 00 00

                 and also when this information is not reported such as in the below sample output

                {869} normal block at 0x005E8998, 4 bytes long.
                 Data: <    > CD CD CD CD
                {868} normal block at 0x005E8848, 4 bytes long.
                 Data: <    > F5 01 00 00

                 */

                #region regexLeakInformation

                // (?:([\\:\w\rA-z.]*?)([\w\d.]*)\((\d{1,})\)\s:\s)?\{(\d{1,})\}[\w\s\d]*,\s(\d{1,})[\s\w.]*\n(.*?)(?=$|(?:[\\\w.:]*\(\d{1,}\)\s:\s)?\{\d{1,}\d)
                //
                // Options: Case insensitive; Exact spacing; Dot matches line breaks; ^$ don't match at line breaks; Numbered capture
                //
                // Match the regular expression below «(?:([\\:\w\rA-z.]*?)([\w\d.]*)\((\d{1,})\)\s:\s)?»
                //    Between zero and one times, as many times as possible, giving back as needed (greedy) «?»
                //    Match the regex below and capture its match into backreference number 1 «([\\:\w\rA-z.]*?)»
                //       Match a single character present in the list below «[\\:\w\rA-z.]*?»
                //          Between zero and unlimited times, as few times as possible, expanding as needed (lazy) «*?»
                //          The backslash character «\\»
                //          The literal character “:” «:»
                //          A “word character” (Unicode; any letter or ideograph, digit, connector punctuation) «\w»
                //          The carriage return character «\r»
                //          A character in the range between “A” and “z” (case insensitive) «A-z»
                //          The literal character “.” «.»
                //    Match the regex below and capture its match into backreference number 2 «([\w\d.]*)»
                //       Match a single character present in the list below «[\w\d.]*»
                //          Between zero and unlimited times, as many times as possible, giving back as needed (greedy) «*»
                //          A “word character” (Unicode; any letter or ideograph, digit, connector punctuation) «\w»
                //          A “digit” (0–9 in any Unicode script) «\d»
                //          The literal character “.” «.»
                //    Match the character “(” literally «\(»
                //    Match the regex below and capture its match into backreference number 3 «(\d{1,})»
                //       Match a single character that is a “digit” (0–9 in any Unicode script) «\d{1,}»
                //          Between one and unlimited times, as many times as possible, giving back as needed (greedy) «{1,}»
                //    Match the character “)” literally «\)»
                //    Match a single character that is a “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s»
                //    Match the character “:” literally «:»
                //    Match a single character that is a “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s»
                // Match the character “{” literally «\{»
                // Match the regex below and capture its match into backreference number 4 «(\d{1,})»
                //    Match a single character that is a “digit” (0–9 in any Unicode script) «\d{1,}»
                //       Between one and unlimited times, as many times as possible, giving back as needed (greedy) «{1,}»
                // Match the character “}” literally «\}»
                // Match a single character present in the list below «[\w\s\d]*»
                //    Between zero and unlimited times, as many times as possible, giving back as needed (greedy) «*»
                //    A “word character” (Unicode; any letter or ideograph, digit, connector punctuation) «\w»
                //    A “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s»
                //    A “digit” (0–9 in any Unicode script) «\d»
                // Match the character “,” literally «,»
                // Match a single character that is a “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s»
                // Match the regex below and capture its match into backreference number 5 «(\d{1,})»
                //    Match a single character that is a “digit” (0–9 in any Unicode script) «\d{1,}»
                //       Between one and unlimited times, as many times as possible, giving back as needed (greedy) «{1,}»
                // Match a single character present in the list below «[\s\w.]*»
                //    Between zero and unlimited times, as many times as possible, giving back as needed (greedy) «*»
                //    A “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s»
                //    A “word character” (Unicode; any letter or ideograph, digit, connector punctuation) «\w»
                //    The literal character “.” «.»
                // Match the line feed character «\n»
                // Match the regex below and capture its match into backreference number 6 «(.*?)»
                //    Match any single character «.*?»
                //       Between zero and unlimited times, as few times as possible, expanding as needed (lazy) «*?»
                // Assert that the regex below can be matched, starting at this position (positive lookahead) «(?=$|(?:[\\\w.:]*\(\d{1,}\)\s:\s)?\{\d{1,}\d)»
                //    Match this alternative (attempting the next alternative only if this one fails) «$»
                //       Assert position at the end of the string, or before the line break at the end of the string, if any (line feed) «$»
                //    Or match this alternative (the entire group fails if this one fails to match) «(?:[\\\w.:]*\(\d{1,}\)\s:\s)?\{\d{1,}\d»
                //       Match the regular expression below «(?:[\\\w.:]*\(\d{1,}\)\s:\s)?»
                //          Between zero and one times, as many times as possible, giving back as needed (greedy) «?»
                //          Match a single character present in the list below «[\\\w.:]*»
                //             Between zero and unlimited times, as many times as possible, giving back as needed (greedy) «*»
                //             The backslash character «\\»
                //             A “word character” (Unicode; any letter or ideograph, digit, connector punctuation) «\w»
                //             A single character from the list “.:” «.:»
                //          Match the character “(” literally «\(»
                //          Match a single character that is a “digit” (0–9 in any Unicode script) «\d{1,}»
                //             Between one and unlimited times, as many times as possible, giving back as needed (greedy) «{1,}»
                //          Match the character “)” literally «\)»
                //          Match a single character that is a “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s»
                //          Match the character “:” literally «:»
                //          Match a single character that is a “whitespace character” (any Unicode separator, tab, line feed, carriage return, vertical tab, form feed, next line) «\s»
                //       Match the character “{” literally «\{»
                //       Match a single character that is a “digit” (0–9 in any Unicode script) «\d{1,}»
                //          Between one and unlimited times, as many times as possible, giving back as needed (greedy) «{1,}»
                //       Match a single character that is a “digit” (0–9 in any Unicode script) «\d»

                #endregion regexLeakInformation

                Match matchLeakInformation = regexLeakInformation.Match(leakInformation);
                while (matchLeakInformation.Success)
                {
                    LogEntryMemoryLeak leak = new LogEntryMemoryLeak();

                    result.LogEntries.Add(leak);

                    //Capturing group 1,2 and 3 will have the 'Success' property false in case the C++ new operator has not been replaced via the macro

                    // Temporary variable used to try and parse unsigned integer values;
                    uint value = 0;

                    if (matchLeakInformation.Groups[1].Success && matchLeakInformation.Groups[2].Success && matchLeakInformation.Groups[3].Success)
                    {
                        leak.LeakSourceFilePath = matchLeakInformation.Groups[1].Value;

                        leak.LeakSourceFileName = matchLeakInformation.Groups[2].Value;

                        if (uint.TryParse(matchLeakInformation.Groups[3].Value, out value))
                        {
                            leak.LeakLineNumber = value;
                        }

                        leak.LeakSourceFileAndLineNumberReportingActive = true;
                    }
                    else
                    {
                        leak.LeakSourceFileAndLineNumberReportingActive = false;
                    }

                    if (uint.TryParse(matchLeakInformation.Groups[4].Value, out value))
                    {
                        leak.LeakMemoryAllocationNumber = value;
                    }

                    if (uint.TryParse(matchLeakInformation.Groups[5].Value, out value))
                    {
                        leak.LeakSizeInBytes = value;
                    }

                    leak.LeakLeakedDataContents = matchLeakInformation.Groups[6].Value;

                    matchLeakInformation = matchLeakInformation.NextMatch();
                }   
            }
        }

        private static void RegisterStandardOutputMessage(string output, TestResultCollection collection)
        {
            if (!string.IsNullOrEmpty(output))
            {
                foreach (TestResult result in collection)
                {
                    result.LogEntries.Add(new LogEntryStandardOutputMessage(output));
                }
            }
        }

        private static string StreamToString(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}