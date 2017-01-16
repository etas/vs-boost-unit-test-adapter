// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using System.Text;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Boost Test Result Parsing Utility.
    /// </summary>
    public static class BoostTestResultParser
    {
        /// <summary>
        /// Parses the Xml report and log file as specified within the provided
        /// BoostTestRunnerCommandLineArgs instance.
        /// </summary>
        /// <param name="args">The BoostTestRunnerCommandLineArgs which specify the report and log file.</param>
        /// <param name="settings">The BoostTestAdapterSettings which specify adapter specific settings.</param>
        public static IDictionary<string, TestResult> Parse(BoostTestRunnerCommandLineArgs args, BoostTestAdapterSettings settings)
        {
            var results = new Dictionary<string, TestResult>();

            Parse(GetReportParser(args, results));
            Parse(GetLogParser(args, results));
            Parse(GetStandardOutputParser(args, settings, results));
            Parse(GetStandardErrorParser(args, settings, results));

            return results;
        }

        /// <summary>
        /// Applies the factory result and parses the intended input
        /// </summary>
        /// <param name="result">The parser factory result</param>
        private static void Parse(ParserFactoryResult result)
        {
            if ((result != null) && (result.Parser != null) && (!string.IsNullOrEmpty(result.SourceFilePath)))
            {
                result.Parser.Parse(ReadAllText(result.SourceFilePath));
            }
        }

        #region IBoostTestResultOutput Factory Methods

        /// <summary>
        /// A convenient pair to associate a parser with its input
        /// </summary>
        private class ParserFactoryResult
        {
            public IBoostTestResultParser Parser { get; set; }
            public string SourceFilePath { get; set; }
        }

        /// <summary>
        /// Factory method which provides the report IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <param name="results">The test result container indexed by test fully qualified name</param>
        /// <returns>An IBoostTestResultParser/Source pair or null if one cannot be identified from the provided arguments</returns>
        private static ParserFactoryResult GetReportParser(BoostTestRunnerCommandLineArgs args, IDictionary<string, TestResult> results)
        {
            if (args.ReportFormat != OutputFormat.XML)
            {
                return null;
            }

            string report = args.ReportFile;
            if (!string.IsNullOrEmpty(report) && (File.Exists(report)))
            {
                return new ParserFactoryResult()
                {
                    Parser = new BoostXmlReport(results),
                    SourceFilePath = report
                };
            }

            return null;
        }

        /// <summary>
        /// Factory method which provides the log IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <param name="results">The test result container indexed by test fully qualified name</param>
        /// <returns>An IBoostTestResultParser/Source pair or null if one cannot be identified from the provided arguments</returns>
        private static ParserFactoryResult GetLogParser(BoostTestRunnerCommandLineArgs args, IDictionary<string, TestResult> results)
        {
            if (args.LogFormat != OutputFormat.XML)
            {
                return null;
            }

            string log = args.LogFile;
            if (!string.IsNullOrEmpty(log) && (File.Exists(log)))
            {
                return new ParserFactoryResult()
                {
                    Parser = new BoostXmlLog(results),
                    SourceFilePath = log
                };
            }

            return null;
        }

        /// <summary>
        /// Factory method which provides the standard output IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs and BoostTestAdapterSettings
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <param name="settings">The run time settings which were used to generate the test results</param>
        /// <param name="results">The test result container indexed by test fully qualified name</param>
        /// <returns>An IBoostTestResultParser/Source pair or null if one cannot be identified from the provided arguments</returns>
        private static ParserFactoryResult GetStandardOutputParser(BoostTestRunnerCommandLineArgs args, BoostTestAdapterSettings settings, IDictionary<string, TestResult> results)
        {
            if ((!string.IsNullOrEmpty(args.StandardOutFile)) && (File.Exists(args.StandardOutFile)))
            {
                return new ParserFactoryResult()
                {
                    Parser = new BoostStandardOutput(results)
                    {
                        FailTestOnMemoryLeak = ((settings != null) && (settings.FailTestOnMemoryLeak))
                    },
                    SourceFilePath = args.StandardOutFile
                };
            }

            return null;
        }

        /// <summary>
        /// Factory method which provides the standard error IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs and BoostTestAdapterSettings
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <param name="settings">The run time settings which were used to generate the test results</param>
        /// <param name="results">The test result container indexed by test fully qualified name</param>
        /// <returns>An IBoostTestResultParser/Source pair or null if one cannot be identified from the provided arguments</returns>
        private static ParserFactoryResult GetStandardErrorParser(BoostTestRunnerCommandLineArgs args, BoostTestAdapterSettings settings, IDictionary<string, TestResult> results)
        {
            if ((!string.IsNullOrEmpty(args.StandardErrorFile)) && (File.Exists(args.StandardErrorFile)))
            {
                return new ParserFactoryResult()
                {
                    Parser = new BoostStandardError(results)
                    {
                        FailTestOnMemoryLeak = ((settings != null) && (settings.FailTestOnMemoryLeak))
                    },
                    SourceFilePath = args.StandardErrorFile
                };
            }

            return null;
        }

        /// <summary>
        /// Reads the contents of the file located at the provided path as an ISO 8859-1 encoded string
        /// </summary>
        /// <param name="path">The file path to read</param>
        /// <returns>The contents of the file as an ISO 8859-1 encoded string</returns>
        private static string ReadAllText(string path)
        {
            var enc = Encoding.GetEncoding("iso-8859-1");
            enc = (Encoding) enc.Clone();
            enc.EncoderFallback = new EncoderReplacementFallback(string.Empty);

            return File.ReadAllText(path, enc);
        }

        #endregion IBoostTestResultOutput Factory Methods
    }
}