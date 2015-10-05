// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Boost.Runner;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestRunnerCommandLineArgsTest
    {
        #region Tests

        /// <summary>
        /// Default configuration of a command-line arguments structure.
        /// 
        /// Test aims:
        ///     - Ensure that a default command-line arguments structure does not generate a command-line string.
        /// </summary>
        [Test]
        public void DefaultCommandLineArgs()
        {
            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs();
            Assert.That(args.ToString(), Is.Empty);
        }

        /// <summary>
        /// Non-default configuration of a command-line arguments structure.
        /// 
        /// Test aims:
        ///     - Ensure that all non-default command-line arguments are listed accordingly.
        /// </summary>
        [Test]
        public void SampleCommandLineArgs()
        {
            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs();

            args.Tests.Add("test");
            args.Tests.Add("suite/*");

            args.LogFormat = OutputFormat.XML;
            args.LogLevel = LogLevel.TestSuite;
            args.LogFile = "log.xml";

            args.ReportFormat = OutputFormat.XML;
            args.ReportLevel = ReportLevel.Detailed;
            args.ReportFile = "report.xml";

            args.DetectMemoryLeaks = 0;

            args.StandardOutFile = "stdout.log";
            args.StandardErrorFile = "stderr.log";

            Assert.That(args.ToString(), Is.EqualTo("\"--run_test=test,suite/*\" \"--log_format=xml\" \"--log_level=test_suite\" \"--log_sink=log.xml\" \"--report_format=xml\" \"--report_level=detailed\" \"--report_sink=report.xml\" \"--detect_memory_leak=0\" > \"stdout.log\" 2> \"stderr.log\""));
        }

        #endregion Tests
    }
}