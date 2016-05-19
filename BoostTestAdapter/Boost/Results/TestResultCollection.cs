// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Settings;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Collection of Boost Test Results.
    /// </summary>
    public class TestResultCollection : IEnumerable<TestResult>
    {
        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TestResultCollection()
        {
            this.Results = new Dictionary<string, TestResult>();
        }

        #endregion Constructors

        #region Properties

        private Dictionary<string, TestResult> Results { get; set; }

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Returns the result for the requested TestUnit or null if not found.
        /// </summary>
        /// <param name="unit">TestUnit to lookup.</param>
        /// <returns>The results associated with the requested TestUnit or null if not found.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public TestResult this[TestUnit unit]
        {
            get
            {
                Utility.Code.Require(unit, "unit");
                return this[unit.FullyQualifiedName];
            }

            set
            {
                Utility.Code.Require(unit, "unit");
                this[unit.FullyQualifiedName] = value;
            }
        }

        /// <summary>
        /// Returns the result for the requested TestUnit fully qualified name or null if not found.
        /// </summary>
        /// <param name="fullyQualifiedName">The TestUnit's fully qualified name to lookup.</param>
        /// <returns>The results associated with the requested TestUnit fully qualified name or null if not found.</returns>
        public TestResult this[string fullyQualifiedName]
        {
            get
            {
                TestResult value = null;
                if (this.Results.TryGetValue(fullyQualifiedName, out value))
                {
                    return value;
                }

                return null;
            }

            set
            {
                this.Results[fullyQualifiedName] = value;
            }
        }

        #endregion Indexers

        /// <summary>
        /// Parses the Xml report and log file as specified within the provided
        /// BoostTestRunnerCommandLineArgs instance.
        /// </summary>
        /// <param name="args">The BoostTestRunnerCommandLineArgs which specify the report and log file.</param>
        /// <param name="settings">The BoostTestAdapterSettings which specify adapter specific settings.</param>
        public void Parse(BoostTestRunnerCommandLineArgs args, BoostTestAdapterSettings settings)
        {
            IEnumerable<IBoostTestResultOutput> parsers = Enumerable.Empty<IBoostTestResultOutput>();

            try
            {
                parsers = new IBoostTestResultOutput[] {
                    GetReportParser(args),
                    GetLogParser(args),
                    GetStandardOutput(args, settings),
                    GetStandardError(args, settings)
                };

                Parse(parsers);
            }
            finally
            {
                foreach (IBoostTestResultOutput parser in parsers)
                {
                    if (parser != null)
                    {
                        parser.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Parses the collection of BoostTestResultOutput. Results are stored
        /// within this instance.
        /// </summary>
        /// <param name="output">The Boost Test output which will be parsed.</param>
        public void Parse(IEnumerable<IBoostTestResultOutput> output)
        {
            Utility.Code.Require(output, "output");

            foreach (IBoostTestResultOutput parser in output)
            {
                if (parser != null)
                {
                    parser.Parse(this);
                }
            }
        }

        #region IBoostTestResultOutput Factory Methods

        /// <summary>
        /// Factory method which provides the report IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <returns>An IBoostTestResultOutput or null if one cannot be identified from the provided arguments</returns>
        private static IBoostTestResultOutput GetReportParser(BoostTestRunnerCommandLineArgs args)
        {
            string report = args.ReportFile;
            if (!string.IsNullOrEmpty(report))
            {
                if (args.ReportFormat == OutputFormat.XML)
                {
                    return new BoostXmlReport(args.ReportFile);
                }
            }

            return null;
        }

        /// <summary>
        /// Factory method which provides the log IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <returns>An IBoostTestResultOutput or null if one cannot be identified from the provided arguments</returns>
        private static IBoostTestResultOutput GetLogParser(BoostTestRunnerCommandLineArgs args)
        {
            string log = args.LogFile;
            if (!string.IsNullOrEmpty(log))
            {
                if (args.LogFormat == OutputFormat.XML)
                {
                    return new BoostXmlLog(args.LogFile);
                }
            }

            return null;
        }

        /// <summary>
        /// Factory method which provides the standard output IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs and BoostTestAdapterSettings
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <param name="settings">The run time settings which were used to generate the test results</param>
        /// <returns>An IBoostTestResultOutput or null if one cannot be identified from the provided arguments</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static IBoostTestResultOutput GetStandardOutput(BoostTestRunnerCommandLineArgs args, BoostTestAdapterSettings settings)
        {
            if ((!string.IsNullOrEmpty(args.StandardOutFile)) && (File.Exists(args.StandardOutFile)))
            {
                return new BoostStandardOutput(args.StandardOutFile)
                {
                    FailTestOnMemoryLeak = settings.FailTestOnMemoryLeak
                };
            }

            return null;
        }

        /// <summary>
        /// Factory method which provides the standard error IBoostTestResultOutput based on the provided BoostTestRunnerCommandLineArgs and BoostTestAdapterSettings
        /// </summary>
        /// <param name="args">The command line args which were used to generate the test results</param>
        /// <param name="settings">The run time settings which were used to generate the test results</param>
        /// <returns>An IBoostTestResultOutput or null if one cannot be identified from the provided arguments</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static IBoostTestResultOutput GetStandardError(BoostTestRunnerCommandLineArgs args, BoostTestAdapterSettings settings)
        {
            if ((!string.IsNullOrEmpty(args.StandardErrorFile)) && (File.Exists(args.StandardErrorFile)))
            {
                return new BoostStandardError(args.StandardErrorFile)
                {
                    FailTestOnMemoryLeak = settings.FailTestOnMemoryLeak
                };
            }

            return null;
        }

        #endregion IBoostTestResultOutput Factory Methods

        #region IEnumerable<TestResult>

        public IEnumerator<TestResult> GetEnumerator()
        {
            // Only enumerate over TestCase results since for our general use case, those are the most important.
            return this.Results.Values.Where((result) => { return (result.Unit as TestCase) != null; }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion IEnumerable<TestResult>
    }
}