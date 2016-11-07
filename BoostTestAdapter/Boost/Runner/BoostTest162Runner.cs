// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Globalization;
using BoostTestAdapter.Utility.ExecutionContext;
using BoostTestAdapter.Utility;

using static BoostTestAdapter.BoostTestExecutor;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// An IBoostTestRunner decorator for Boost Test released with Boost 1.62.
    /// Necessary to overcome certain release limitations stemming from command line use.
    /// </summary>
    public class BoostTest162Runner : IBoostTestRunner
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="runner">The runner which is to be adapted for Boost Test for Boost 1.62 use</param>
        public BoostTest162Runner(IBoostTestRunner runner)
        {
            this.Runner = runner;
        }

        #region Properties

        /// <summary>
        /// Base (wrapped) IBoostTestRunner
        /// </summary>
        public IBoostTestRunner Runner { get; private set; }

        #endregion

        #region IBoostTestRunner

        public bool ListContentSupported => this.Runner.ListContentSupported;

        public string Source => this.Runner.Source;

        public void Execute(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings, IProcessExecutionContext executionContext)
        {
            var fixedArgs = args;

            if (args != null)
            {
                // Adapt a copy of the command-line arguments to avoid changing the original
                fixedArgs = AdaptArguments(args.Clone());
            }

            using (var stderr = new TemporaryFile(IsStandardErrorFileDifferent(args, fixedArgs) ? fixedArgs.StandardErrorFile : null))
            {
                this.Runner.Execute(fixedArgs, settings, executionContext);

                // Extract the report output to its intended location
                string source = (fixedArgs == null) ? null : fixedArgs.StandardErrorFile;
                string destination = (args == null) ? null : args.ReportFile;

                if ((source != null) && (destination != null))
                {
                    try
                    {
                        ExtractReport(source, destination, string.IsNullOrEmpty(stderr.Path));
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex, "Failed to extract test report from standard error [{0}] to report file [{1}] ({2})", source, destination, ex.Message);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Adapts the command line environment for Boost Test released in Boost 1.62
        /// </summary>
        /// <remarks>https://github.com/etas/vs-boost-unit-test-adapter/issues/158</remarks>
        /// <param name="args">The command line arguments/environment to adapt</param>
        /// <returns>An adapted 'args'</returns>
        private BoostTestRunnerCommandLineArgs AdaptArguments(BoostTestRunnerCommandLineArgs args)
        {
            // Boost Test for Boost 1.62 causes an incorrect cast issue with the --log_sink (and --report_sink) command line argument
            if (args.Log != Sink.StandardOutput)
            {
                // Make use of environment variables instead of command-line
                // arguments to retain support for Boost Test in Boost 1.61

                string logSink = args.Log.ToString();

                if (args.Log != Sink.StandardError)
                {
                    // Remove the ':' used as the volume separator since the Boost Test framework interprets it as a logger separator
                    logSink = ((Path.IsPathRooted(logSink) && (logSink[1] == ':')) ? logSink.Substring(2) : logSink);
                }

                // BOOST_TEST_LOGGER (--logger) overrides --log_sink, --log_format and --log_level
                args.Environment["BOOST_TEST_LOGGER"] = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0},{1},{2}",
                    BoostTestRunnerCommandLineArgs.OutputFormatToString(args.LogFormat),
                    BoostTestRunnerCommandLineArgs.LogLevelToString(args.LogLevel),
                    logSink
                );
            }
            
            // Boost Test (Boost 1.62) --report_sink workaround - force report output to standard error due to cast issue with --report_sink
            args.Report = Sink.StandardError;
            if (string.IsNullOrEmpty(args.StandardErrorFile))
            {
                args.StandardErrorFile = TestPathGenerator.Generate(this.Source, FileExtensions.StdErrFile);
            }

            return args;
        }

        /// <summary>
        /// Determines whether or not the 'StandarderrorFile' property is different from the 2 BoostTestRunnerCommandLineArgs instances
        /// </summary>
        /// <param name="lhs">the left-hand side instance to compare</param>
        /// <param name="rhs">the right-hand side instance to compare</param>
        /// <returns>true if the 'StandarderrorFile' property is different; false otherwise</returns>
        private static bool IsStandardErrorFileDifferent(BoostTestRunnerCommandLineArgs lhs, BoostTestRunnerCommandLineArgs rhs)
        {
            return (lhs != rhs) && (lhs != null) && (rhs != null) && (lhs.StandardErrorFile != rhs.StandardErrorFile);
        }

        private const string _beginTestResultTag = "<TestResult>";
        private const string _endTestResultTag = "</TestResult>";
        
        /// <summary>
        /// Extracts the report content out of the original file, possibly containing other content (e.g. custom output, memory leak reports)
        /// </summary>
        /// <param name="source">The source file containing the report content</param>
        /// <param name="destination">The destination file which will contain only the report content</param>
        /// <param name="retainRemainder">Flag which states if the remainder of the source file is to be retained</param>
        private static void ExtractReport(string source, string destination, bool retainRemainder)
        {
            string content = File.ReadAllText(source);

            int begin = content.IndexOf(_beginTestResultTag, StringComparison.Ordinal);
            int end = -1;

            if (begin >= 0)
            {
                end = content.IndexOf(_endTestResultTag, begin, StringComparison.Ordinal);
            }

            if (end > begin)
            {
                end += _endTestResultTag.Length;

                // Place the report content in the destination file
                string report = content.Substring(begin, (end - begin));
                File.WriteAllText(destination, report);

                if (retainRemainder)
                {
                    // Remove the report content from the source file
                    string remainder = (content.Substring(0, begin) + content.Substring(end));
                    File.WriteAllText(source, remainder);
                }
            }
        }
    }
}