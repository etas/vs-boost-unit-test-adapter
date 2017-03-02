// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Boost.Runner;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestRunnerCommandLineArgsTest
    {
        #region Utility Methods

        /// <summary>
        /// Generates a dummy fully qualified path for the provided filename
        /// </summary>
        /// <param name="filename">The filename to generate a fully qualified version for it</param>
        /// <returns>A dummy fully qualified path</returns>
        private static string GenerateFullyQualifiedPath(string filename)
        {
            return @"C:\Temp\" + filename;
        }
        
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
            args.LogFile = GenerateFullyQualifiedPath("log.xml");

            args.ReportFormat = OutputFormat.XML;
            args.ReportLevel = ReportLevel.Detailed;
            args.ReportFile = GenerateFullyQualifiedPath("report.xml");

            args.DetectMemoryLeaks = 0;

            args.CatchSystemErrors = false;
            args.DetectFPExceptions = true;

            args.StandardOutFile = GenerateFullyQualifiedPath("stdout.log");
            args.StandardErrorFile = GenerateFullyQualifiedPath("stderr.log");

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
        /// "BUTA" environment variable present by default in the environment variable collection
        /// 
        /// Test aims:
        ///     - Ensure that the "BUTA" environment is present by default in the enviorment variable collection.
        /// </summary>
        [Test]
        public void BUTAEnviormentVariablePresent()
        {
            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs();

            var oExpectedEnviormentVariables = new Dictionary<string, string>()
            {
                { "BUTA", "1" }
            };

            CollectionAssert.AreEqual(oExpectedEnviormentVariables, args.Environment);
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
            // serge: boost 1.60 requires uppercase input
            Assert.That(args.ToString(), Is.EqualTo("\"--run_test=test,suite/*\" \"--catch_system_errors=no\" \"--log_format=XML\" \"--log_level=test_suite\" \"--log_sink="
                + GenerateFullyQualifiedPath("log.xml") + "\" \"--report_format=XML\" \"--report_level=detailed\" \"--report_sink="
                + GenerateFullyQualifiedPath("report.xml") + "\" \"--detect_memory_leak=0\" \"--detect_fp_exceptions=yes\" > \"" 
                + GenerateFullyQualifiedPath("stdout.log") + "\" 2> \""
                + GenerateFullyQualifiedPath("stderr.log") + "\""));
        }

        /// <summary>
        /// --log_sink and --report_sink can be specified to standard out and standard error
        /// 
        /// Test aims:
        ///     - Assert that: it is possible to specify standard output/error for sink arguments
        /// </summary>
        [Test]
        public void StdOutStdErrSink()
        {
            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs()
            {
                Log = Sink.StandardError,
                Report = Sink.StandardOutput
            };

            Assert.That(args.ToString(), Is.EqualTo("\"--log_sink=stderr\" \"--report_sink=stdout\""));
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
            Assert.That(args.ColorOutput, Is.EqualTo(clone.ColorOutput));
            Assert.That(args.ResultCode, Is.EqualTo(clone.ResultCode));
            Assert.That(args.Random, Is.EqualTo(clone.Random));
            Assert.That(args.UseAltStack, Is.EqualTo(clone.UseAltStack));
            Assert.That(args.DetectFPExceptions, Is.EqualTo(clone.DetectFPExceptions));
            Assert.That(args.SavePattern, Is.EqualTo(clone.SavePattern));
            Assert.That(args.ListContent, Is.EqualTo(clone.ListContent));

            Assert.That(args.ToString(), Is.EqualTo(clone.ToString()));
            Assert.That(args.Environment, Is.EqualTo(clone.Environment));
        }

        /// <summary>
        /// Based on how a file path is provided, the class tries to root the path if possible
        /// 
        /// Test aims:
        ///     - The file path is rooted if possible.
        /// </summary>
        [Test]
        public void FilePaths()
        {
            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs();

            args.LogFile = "log.xml";
            Assert.That(args.LogFile, Is.EqualTo("log.xml"));
            Assert.That(args.ToString(), Is.EqualTo("\"--log_sink=log.xml\""));

            args.WorkingDirectory = @"C:\";
            Assert.That(args.LogFile, Is.EqualTo(@"C:\log.xml"));
            Assert.That(args.ToString(), Is.EqualTo("\"--log_sink=C:\\log.xml\""));

            args.LogFile = @"D:\Temp\log.xml";
            Assert.That(args.LogFile, Is.EqualTo(@"D:\Temp\log.xml"));
            Assert.That(args.ToString(), Is.EqualTo("\"--log_sink=D:\\Temp\\log.xml\""));
        }

        /// <summary>
        /// Verifies that when requesting list content, the command line is generated accordingly
        /// 
        /// Test aims:
        ///     - --list_content command line arguments are correctly generated
        /// </summary>
        [Test]
        public void ListContentCommandLineArgs()
        {
            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs();

            args.ListContent = ListContentFormat.DOT;

            args.StandardOutFile = @"C:\Temp\list_content.dot.out";
            args.StandardErrorFile = @"C:\Temp\list_content.dot.err";

            const string expected = "\"--list_content=DOT\" > \"C:\\Temp\\list_content.dot.out\" 2> \"C:\\Temp\\list_content.dot.err\"";
            Assert.That(args.ToString(), Is.EqualTo(expected));

            args.ReportFormat = OutputFormat.XML;
            args.ReportFile = @"C:\Temp\list_content.report.xml";
            
            // list content only includes the --list_content and the output redirection commands
            Assert.That(args.ToString(), Is.EqualTo(expected));
        }

        #endregion Tests
    }
}