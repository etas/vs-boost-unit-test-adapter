using BoostTestAdapter.Boost.Runner;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestRunnerCommandLineArgsTest
    {
        #region Utility Methods

        /// <summary>
        /// Generates a command line args instance with pre-determined values.
        /// </summary>
        /// <returns>A new BoostTestRunnerCommandLineArgs instance populated with pre-determined values.</returns>
        private static BoostTestRunnerCommandLineArgs GenerateCommandLineArgs()
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

            args.CatchSystemErrors = false;
            args.DetectFPExceptions = true;

            args.StandardOutFile = "stdout.log";
            args.StandardErrorFile = "stderr.log";

            return args;
        }

        #endregion Utility Methods

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
            BoostTestRunnerCommandLineArgs args = GenerateCommandLineArgs();

            Assert.That(args.ToString(), Is.EqualTo("\"--run_test=test,suite/*\" \"--catch_system_errors=no\" \"--log_format=xml\" \"--log_level=test_suite\" \"--log_sink=log.xml\" \"--report_format=xml\" \"--report_level=detailed\" \"--report_sink=report.xml\" \"--detect_memory_leak=0\" \"--detect_fp_exceptions=yes\" > \"stdout.log\" 2> \"stderr.log\""));
        }

        /// <summary>
        /// Verifies that cloning a command-line args structure is possible and correct.
        /// 
        /// Test aims:
        ///     - Ensure that the command-line args structure is cloneable.
        /// </summary>
        [Test]
        public void CloneCommandLineArgs()
        {
            BoostTestRunnerCommandLineArgs args = GenerateCommandLineArgs();
            BoostTestRunnerCommandLineArgs clone = args.Clone();
            
            Assert.That(args.Tests, Is.EqualTo(clone.Tests));
            Assert.That(args.WorkingDirectory, Is.EqualTo(clone.WorkingDirectory));
            Assert.That(args.LogFile, Is.EqualTo(clone.LogFile));
            Assert.That(args.LogFormat, Is.EqualTo(clone.LogFormat));
            Assert.That(args.LogLevel, Is.EqualTo(clone.LogLevel));
            Assert.That(args.ReportFile, Is.EqualTo(clone.ReportFile));
            Assert.That(args.ReportFormat, Is.EqualTo(clone.ReportFormat));
            Assert.That(args.ReportLevel, Is.EqualTo(clone.ReportLevel));
            Assert.That(args.DetectMemoryLeaks, Is.EqualTo(clone.DetectMemoryLeaks));
            Assert.That(args.StandardErrorFile, Is.EqualTo(clone.StandardErrorFile));
            Assert.That(args.StandardOutFile, Is.EqualTo(clone.StandardOutFile));
            
            Assert.That(args.ShowProgress, Is.EqualTo(clone.ShowProgress));
            Assert.That(args.BuildInfo, Is.EqualTo(clone.BuildInfo));
            Assert.That(args.AutoStartDebug, Is.EqualTo(clone.AutoStartDebug));
            Assert.That(args.CatchSystemErrors, Is.EqualTo(clone.CatchSystemErrors));
            Assert.That(args.BreakExecPath, Is.EqualTo(clone.BreakExecPath));
            Assert.That(args.ColorOutput, Is.EqualTo(clone.ColorOutput));
            Assert.That(args.ResultCode, Is.EqualTo(clone.ResultCode));
            Assert.That(args.Random, Is.EqualTo(clone.Random));
            Assert.That(args.UseAltStack, Is.EqualTo(clone.UseAltStack));
            Assert.That(args.DetectFPExceptions, Is.EqualTo(clone.DetectFPExceptions));
            Assert.That(args.SavePattern, Is.EqualTo(clone.SavePattern));
            Assert.That(args.ListContent, Is.EqualTo(clone.ListContent));

            Assert.That(args.ToString(), Is.EqualTo(clone.ToString()));
        }

        #endregion Tests
    }
}