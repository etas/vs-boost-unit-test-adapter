// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Output format options for Boost Test output.
    /// Reference: http://www.boost.org/doc/libs/1_43_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html
    /// </summary>
    public enum OutputFormat
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HRF")]
        HRF, // Human Readable Format
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "XML")]
        XML,

        Default = HRF
    }

    /// <summary>
    /// Log level enumeration
    /// Reference: http://www.boost.org/doc/libs/1_43_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html
    /// </summary>
    public enum LogLevel
    {
        All,
        Success,
        TestSuite,
        Message,
        Warning,
        Error,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
        CppException,
        SystemError,
        FatalError,
        Nothing,

        Default = Error
    }

    /// <summary>
    /// Report level enumeration
    /// Reference: http://www.boost.org/doc/libs/1_43_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html
    /// </summary>
    public enum ReportLevel
    {
        None,
        Confirm,
        Short,
        Detailed,

        Default = Confirm
    }

    /// <summary>
    /// Output format options for Boost Test --list_content output.
    /// Reference: http://www.boost.org/doc/libs/1_60_0/libs/test/doc/html/boost_test/utf_reference/rt_param_reference/list_content.html
    /// </summary>
    public enum ListContentFormat
    {
        HRF, // Human Readable Format
        DOT,

        Default = HRF
    }

    /// <summary>
    /// An output destination
    /// </summary>
    /// <remarks>Intended for logger or report output</remarks>
    public class Sink
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Private to allow for a more readable interface via the static File method</remarks>
        /// <param name="sink">The output destination</param>
        private Sink(string sink)
        {
            Value = sink;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The output destination
        /// </summary>
        private string Value { get; set; }

        /// <summary>
        /// Standard Output Sink
        /// </summary>
        public static Sink StandardOutput { get; } = new Sink("stdout");

        /// <summary>
        /// Standard Error Sink
        /// </summary>
        public static Sink StandardError { get; } = new Sink("stderr");

        #endregion

        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Factory method which construct a file output sink instance.
        /// </summary>
        /// <param name="path">The path to the output file</param>
        /// <returns>A sink which represents a file output</returns>
        public static Sink File(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if ((path == StandardOutput.ToString()) || (path == StandardError.ToString()))
            {
                throw new ArgumentException("The value of the 'path' argument is a reserved keyword", "path");
            }

            return new Sink(path);
        }
    }
    
    /// <summary>
    /// Aggregates all possible command line options made available by the Boost Test framework.
    /// Reference: http://www.boost.org/doc/libs/1_43_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html
    /// Reference: http://www.boost.org/doc/libs/1_59_0/libs/test/doc/html/boost_test/utf_reference/rt_param_reference.html
    /// </summary>
    public class BoostTestRunnerCommandLineArgs : ICloneable
    {
        #region Constants

        internal const string RunTestArg = "--run_test";

        internal const string LogFormatArg = "--log_format";
        internal const string LogLevelArg = "--log_level";
        internal const string LogSinkArg = "--log_sink";

        internal const string ReportFormatArg = "--report_format";
        internal const string ReportLevelArg = "--report_level";
        internal const string ReportSinkArg = "--report_sink";

        internal const string DetectMemoryLeakArg = "--detect_memory_leak";

        internal const string ShowProgressArg = "--show_progress";
        internal const string BuildInfoArg = "--build_info";
        internal const string AutoStartDebugArg = "--auto_start_dbg";
        internal const string CatchSystemErrorsArg = "--catch_system_errors";
        internal const string ColorOutputArg = "--color_output";
        internal const string ResultCodeArg = "--result_code";
        internal const string RandomArg = "--random";
        internal const string UseAltStackArg = "--use_alt_stack";
        internal const string DetectFPExceptionsArg = "--detect_fp_exceptions";
        internal const string SavePatternArg = "--save_pattern";
        internal const string ListContentArg = "--list_content";
        internal const string HelpArg = "--help";

        internal const string VersionArg = "--version";

        private const string TestSeparator = ",";

        private const char ArgSeparator = ' ';
        private const char ArgValueSeparator = '=';

        private const char RedirectionOperator = '>';
        private const string ErrRedirectionOperator = "2>";

        private const string Yes = "yes";
        private const string No = "no";

        #endregion Constants

        #region Members

        private Sink _log = null;
        private Sink _report = null;

        private string _stdOutFile = null;
        private string _stdErrFile = null;

        #endregion Members

        #region Constructors

        /// <summary>
        /// Default Constructor. Initializes options to their defaults as specified in:
        /// http://www.boost.org/doc/libs/1_43_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html.
        /// </summary>
        public BoostTestRunnerCommandLineArgs()
        {
            this.Tests = new List<string>();

            this.Log = Sink.StandardOutput;
            this.LogFormat = OutputFormat.Default;
            this.LogLevel = LogLevel.Default;

            this.Report = Sink.StandardError;
            this.ReportFormat = OutputFormat.Default;
            this.ReportLevel = ReportLevel.Default;

            this.DetectMemoryLeaks = 1;

            this.ShowProgress = false;
            this.BuildInfo = false;
            this.AutoStartDebug = "no";
            this.CatchSystemErrors = null;
            this.ColorOutput = false;
            this.ResultCode = true;
            this.Random = 0;
            this.UseAltStack = true;
            this.DetectFPExceptions = false;
            this.SavePattern = false;
            this.ListContent = null;
            this.Help = false;
            this.Version = false;

            /* The environment variable "BUTA" is set whenever a test is executed. The purpose
             * is to provide a means for boost unit tests to detect that they are being executed
             * using the boost unit test adapter. One might use this so as for the tests to 
             * increase the verbosity level whenever boost unit tests are executed using the
             * boost unit test adapter.
             */

            this.Environment = new Dictionary<string, string>()
            {
                { "BUTA", "1" }
            };
        }

        #endregion Constructors

        /// <summary>
        /// Specify the process' working directory
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Specifies the process's environment (variables)
        /// </summary>
        public IDictionary<string, string> Environment { get; private set; }

        /// <summary>
        /// List of fully qualified name tests which are to be executed.
        /// </summary>
        public IList<string> Tests { get; private set; }

        /// <summary>
        /// Output format for log information.
        /// </summary>
        public OutputFormat LogFormat { get; set; }

        /// <summary>
        /// Log level.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Output destination to the log which will host Boost Test output.
        /// </summary>
        public Sink Log
        {
            get
            {
                return GetRootedSink(this._log);
            }

            set
            {
                this._log = value;
            }
        }

        /// <summary>
        /// Path (relative to the WorkingDirectory) to the log file which will host Boost Test output.
        /// </summary>
        public string LogFile
        {
            get
            {
                return GetFile(this.Log);
            }

            set
            {
                this.Log = Sink.File(value);
            }
        }

        /// <summary>
        /// Output format for report information.
        /// </summary>
        public OutputFormat ReportFormat { get; set; }

        /// <summary>
        /// Report level.
        /// </summary>
        public ReportLevel ReportLevel { get; set; }

        /// <summary>
        /// Output destination to the report which will host Boost Test report output
        /// </summary>
        public Sink Report
        {
            get
            {
                return GetRootedSink(this._report);
            }

            set
            {
                this._report = value;
            }
        }
        
        /// <summary>
        /// Path (relative to the WorkingDirectory) to the report file which will host Boost Test report output.
        /// </summary>
        public string ReportFile
        {
            get
            {
                return GetFile(this.Report);
            }

            set
            {
                this.Report = Sink.File(value);
            }
        }

        /// <summary>
        /// Set to value greater than 0 to detect memory leaks.
        /// Refer to: http://www.boost.org/doc/libs/1_43_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html.
        /// </summary>
        public uint DetectMemoryLeaks { get; set; }

        /// <summary>
        /// Flag determining whether test execution progress is displayed in standard out.
        /// </summary>
        public bool ShowProgress { get; set; }

        /// <summary>
        /// String determining the debugger to attach in case of system failure.
        /// </summary>
        public string AutoStartDebug { get; set; }

        /// <summary>
        /// Flag which displays Boost.Test test information in standard out.
        /// </summary>
        public bool BuildInfo { get; set; }

        /// <summary>
        /// Determines whether system errors should be caught.
        /// </summary>
        /// <remarks>
        /// Since the default value of '--catch_system_errors' is dependent on Boost.Test's 
        /// '#define BOOST_TEST_DEFAULTS_TO_CORE_DUMP', this value has been set to a optional type
        /// 
        /// Reference: http://www.boost.org/doc/libs/1_60_0/libs/test/doc/html/boost_test/utf_reference/rt_param_reference/catch_system.html
        /// </remarks>
        public bool? CatchSystemErrors { get; set; }

        /// <summary>
        /// States whether standard output text is colour coded.
        /// </summary>
        /// <remarks>Introduced in Boost 1.59 / Boost Test 3</remarks>
        public bool ColorOutput { get; set; }

        /// <summary>
        /// Determines the result code the Boost.Test uses on exit.
        /// </summary>
        public bool ResultCode { get; set; }

        /// <summary>
        /// Random seed which determines test execution order. Positive value implies random order. 0 implies sequential execution.
        /// </summary>
        public uint Random { get; set; }

        /// <summary>
        /// Specifies whether or not the Boost.Test Execution Monitor should employ an alternative stack for signals processing on platforms where they are supported.
        /// </summary>
        public bool UseAltStack { get; set; }

        /// <summary>
        /// Instructs the Boost.Test to break on floating-point exceptions.
        /// </summary>
        public bool DetectFPExceptions { get; set; }

        /// <summary>
        /// Useful for test modules relying on boost::test_tools::output_test_stream. True implies 'save the pattern file'; false implies 'match against a previously stored pattern file'.
        /// </summary>
        /// <remarks>Introduced in Boost 1.59 / Boost Test 3</remarks>
        public bool SavePattern { get; set; }

        /// <summary>
        /// The Boost.Test lists all tests which are to be executed without actually executing the tests.
        /// </summary>
        /// <remarks>Introduced in Boost 1.59 / Boost Test 3</remarks>
        public ListContentFormat? ListContent { get; set; }

        /// <summary>
        /// Help output.
        /// </summary>
        public bool Help { get; set; }

        /// <summary>
        /// Version information output.
        /// </summary>
        public bool Version { get; set; }

        /// <summary>
        /// Path (relative to the WorkingDirectory) to the report file which will host the standard output content.
        /// </summary>
        public string StandardOutFile
        {
            get
            {
                return GetRootedPath(this._stdOutFile);
            }

            set
            {
                this._stdOutFile = value;
            }
        }

        /// <summary>
        /// Path (relative to the WorkingDirectory) to the report file which will host the standard error content.
        /// </summary>
        public string StandardErrorFile
        {
            get
            {
                return GetRootedPath(this._stdErrFile);
            }

            set
            {
                this._stdErrFile = value;
            }
        }
        
        /// <summary>
        /// Provides a string representation of the command line.
        /// </summary>
        /// <returns>The command line as a string.</returns>
        public override string ToString()
        {
            StringBuilder args = new StringBuilder();

            // --help
            if (this.Help)
            {
                AddArgument(HelpArg, args);

                // return immediately since Boost.Test should ignore the rest of the arguments
                return AppendRedirection(args).ToString().TrimEnd();
            }

            // --version
            if (this.Version)
            {
                AddArgument(VersionArg, args);

                // return immediately since Boost.Test should ignore the rest of the arguments
                return AppendRedirection(args).ToString().TrimEnd();
            }

            // --list_content
            if (this.ListContent != null)
            {
                AddArgument(ListContentArg, ListContentFormatToString(this.ListContent.Value), args);

                // return immediately since Boost.Test should ignore the rest of the arguments
                return AppendRedirection(args).ToString().TrimEnd();
            }

            // --run_test=a,b,c
            if (this.Tests.Count > 0)
            {
                AddArgument(RunTestArg, string.Join(TestSeparator, this.Tests), args);
            }

            // --show_progress=yes
            if (this.ShowProgress)
            {
                AddArgument(ShowProgressArg, Yes, args);
            }

            // --build_info=yes
            if (this.BuildInfo)
            {
                AddArgument(BuildInfoArg, Yes, args);
            }

            // --auto_start_dbg=yes
            if (string.IsNullOrEmpty(this.AutoStartDebug))
            {
                AddArgument(AutoStartDebugArg, this.AutoStartDebug, args);
            }

            // --catch_system_errors=yes
            if (this.CatchSystemErrors.HasValue)
            {
                AddArgument(CatchSystemErrorsArg, (this.CatchSystemErrors.Value ? Yes : No), args);
            }

            // --color_output=yes
            if (this.ColorOutput)
            {
                AddArgument(ColorOutputArg, Yes, args);
            }

            // --log_format=xml
            if (this.LogFormat != OutputFormat.Default)
            {
                AddArgument(LogFormatArg, OutputFormatToString(this.LogFormat), args);
            }

            // --log_level=test_suite
            if (this.LogLevel != LogLevel.Default)
            {
                AddArgument(LogLevelArg, LogLevelToString(this.LogLevel), args);
            }

            // --log_sink=log.xml
            if (this.Log != Sink.StandardOutput)
            {
                AddArgument(LogSinkArg, this.Log.ToString(), args);
            }

            // --report_format=xml
            if (this.ReportFormat != OutputFormat.Default)
            {
                AddArgument(ReportFormatArg, OutputFormatToString(this.ReportFormat), args);
            }

            // --report_level=detailed
            if (this.ReportLevel != ReportLevel.Default)
            {
                AddArgument(ReportLevelArg, ReportLevelToString(this.ReportLevel), args);
            }

            // --report_sink=report.xml
            if (this.Report != Sink.StandardError)
            {
                AddArgument(ReportSinkArg, this.Report.ToString(), args);
            }

            // --result_code=no
            if (!this.ResultCode)
            {
                AddArgument(ResultCodeArg, No, args);
            }

            // --random=8
            if (this.Random > 0)
            {
                AddArgument(RandomArg, this.Random.ToString(CultureInfo.InvariantCulture), args);
            }

            // --detect_memory_leak=0
            if (this.DetectMemoryLeaks != 1)
            {
                AddArgument(DetectMemoryLeakArg, this.DetectMemoryLeaks.ToString(CultureInfo.InvariantCulture), args);
            }

            // --use_alt_stack=no
            if (!this.UseAltStack)
            {
                AddArgument(UseAltStackArg, No, args);
            }

            // --detect_fp_exceptions=yes
            if (this.DetectFPExceptions)
            {
                AddArgument(DetectFPExceptionsArg, Yes, args);
            }

            // --save_pattern=yes
            if (this.SavePattern)
            {
                AddArgument(SavePatternArg, Yes, args);
            }

            return AppendRedirection(args).ToString().TrimEnd();
        }

        /// <summary>
        /// Appends the standard output and standard error redirection operators to the command-line
        /// </summary>
        /// <param name="args">The command line representation.</param>
        /// <returns>args</returns>
        private StringBuilder AppendRedirection(StringBuilder args)
        {
            // > std.out
            if (!string.IsNullOrEmpty(this._stdOutFile))
            {
                args.Append(RedirectionOperator).Append(ArgSeparator).Append(Quote(this.StandardOutFile)).Append(ArgSeparator);
            }

            // 2> std.err
            if (!string.IsNullOrEmpty(this._stdErrFile))
            {
                args.Append(ErrRedirectionOperator).Append(ArgSeparator).Append(Quote(this.StandardErrorFile));
            }

            return args;
        }

        /// <summary>
        /// Returns the associated output file for the provided sink.
        /// </summary>
        /// <param name="sink">The output sink.</param>
        /// <returns>The file associated to the provided sink.</returns>
        protected string GetFile(Sink sink)
        {
            if (sink == Sink.StandardOutput)
            {
                return StandardOutFile;
            }
            else if (sink == Sink.StandardError)
            {
                return StandardErrorFile;
            }

            return (sink == null) ? null : GetRootedPath(sink.ToString());
        }

        /// <summary>
        /// Returns a 'rooted' sink for the provided one if the sink happens to be a file.
        /// </summary>
        /// <param name="sink">The sink to root.</param>
        /// <returns>The rooted sink.</returns>
        protected Sink GetRootedSink(Sink sink)
        {
            if ((sink == Sink.StandardOutput) || (sink == Sink.StandardError))
            {
                return sink;
            }

            return (sink == null ? null : Sink.File(GetRootedPath(sink.ToString())));
        }

        /// <summary>
        /// Returns a rooted path (i.e. relative to the working directory) for the provided one.
        /// </summary>
        /// <param name="path">The path to root.</param>
        /// <returns>The rooted path.</returns>
        protected string GetRootedPath(string path)
        {
            if (!string.IsNullOrEmpty(this.WorkingDirectory) && !string.IsNullOrEmpty(path))
            {
                if (!Path.IsPathRooted(path) && !path.StartsWith(this.WorkingDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.Combine(this.WorkingDirectory, path);
                }
            }

            return path;
        }
        
        /// <summary>
        /// Provides a (valid) string representation of the provided OutputFormat.
        /// </summary>
        /// <param name="value">The value to serialize to string.</param>
        /// <returns>A (valid) string representation of the provided OutputFormat.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal static string OutputFormatToString(OutputFormat value)
        {
            // serge: boost 1.60 requires uppercase input
            return value.ToString();
        }

        /// <summary>
        /// Provides a (valid) string representation of the provided LogLevel.
        /// </summary>
        /// <param name="value">The value to serialize to string.</param>
        /// <returns>A (valid) string representation of the provided LogLevel.</returns>
        internal static string LogLevelToString(LogLevel value)
        {
            return LevelToString(value.ToString());
        }

        /// <summary>
        /// Provides a (valid) string representation of the provided ListContentFormat.
        /// </summary>
        /// <param name="value">The value to serialize to string.</param>
        /// <returns>A (valid) string representation of the provided ListContentFormat.</returns>
        private static string ListContentFormatToString(ListContentFormat value)
        {
            return value.ToString();
        }
        
        /// <summary>
        /// Provides a (valid) string representation of the provided ReportLevel.
        /// </summary>
        /// <param name="value">The value to serialize to string.</param>
        /// <returns>A (valid) string representation of the provided ReportLevel.</returns>
        private static string ReportLevelToString(ReportLevel value)
        {
            return LevelToString(value.ToString());
        }

        /// <summary>
        /// Provides a (valid) string representation of the provided LogLevel/ReportLevel.
        /// Changes from pascal case to underscore separated lower case.
        /// </summary>
        /// <param name="value">The value to serialize to string.</param>
        /// <returns>A (valid) string representation of the provided LogLevel/ReportLevel.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static string LevelToString(string value)
        {
            return Regex.Replace(value, "([A-Z])", "_$1").Substring(1).ToLowerInvariant();
        }

        /// <summary>
        /// Adds a no-value command-line argument to the provided StringBuilder.
        /// </summary>
        /// <param name="prefix">The command line option.</param>
        /// <param name="host">The StringBuilder which will host the result.</param>
        /// <returns>host</returns>
        protected static StringBuilder AddArgument(string prefix, StringBuilder host)
        {
            return AddArgument(prefix, string.Empty, host);
        }

        /// <summary>
        /// Adds a command-line argument to the provided StringBuilder.
        /// </summary>
        /// <param name="prefix">The command line option.</param>
        /// <param name="value">The command line option's value.</param>
        /// <param name="host">The StringBuilder which will host the result.</param>
        /// <returns>host</returns>
        protected static StringBuilder AddArgument(string prefix, string value, StringBuilder host)
        {
            return AddArgument(prefix, ArgValueSeparator, value, host);
        }

        /// <summary>
        /// Adds a command-line argument to the provided StringBuilder.
        /// </summary>
        /// <param name="prefix">The command line option.</param>
        /// <param name="separator">The separator to use between the prefix and the value.</param>
        /// <param name="value">The command line option's value.</param>
        /// <param name="host">The StringBuilder which will host the result.</param>
        /// <returns>host</returns>
        protected static StringBuilder AddArgument(string prefix, char separator, string value, StringBuilder host)
        {
            Utility.Code.Require(host, "host");

            if (separator == ' ')
            {
                prefix = Quote(prefix);
                value = Quote(value);
            }
            else
            {
                host.Append('"');
            }

            host.Append(prefix);

            if (!string.IsNullOrEmpty(value))
            {
                host.Append(separator).Append(value);
            }

            if (separator != ' ')
            {
                host.Append('"');
            }

            return host.Append(ArgSeparator);
        }

        /// <summary>
        /// Quotes the provided string within double quotation marks.
        /// </summary>
        /// <param name="value">The string to quote.</param>
        /// <returns>The quoted value.</returns>
        private static string Quote(string value)
        {
            return (string.IsNullOrEmpty(value)) ? value : ('"' + value + '"');
        }

        #region ICloneable

        public BoostTestRunnerCommandLineArgs Clone()
        {
            BoostTestRunnerCommandLineArgs clone = new BoostTestRunnerCommandLineArgs();

            clone.WorkingDirectory = this.WorkingDirectory;

            // Shallow copy
            clone.Environment = new Dictionary<string, string>(this.Environment);

            // Deep copy
            clone.Tests = new List<string>(this.Tests);

            clone.LogFormat = this.LogFormat;
            clone.LogLevel = this.LogLevel;
            clone._log = this._log;

            clone.ReportFormat = this.ReportFormat;
            clone.ReportLevel = this.ReportLevel;
            clone._report = this._report;

            clone.DetectMemoryLeaks = this.DetectMemoryLeaks;

            clone._stdOutFile = this._stdOutFile;
            clone._stdErrFile = this._stdErrFile;

            clone.ShowProgress = this.ShowProgress;
            clone.BuildInfo = this.BuildInfo;
            clone.AutoStartDebug = this.AutoStartDebug;
            clone.CatchSystemErrors = this.CatchSystemErrors;

            clone.ColorOutput = this.ColorOutput;
            clone.ResultCode = this.ResultCode;
            clone.Random = this.Random;
            clone.UseAltStack = this.UseAltStack;
            clone.DetectFPExceptions = this.DetectFPExceptions;
            clone.SavePattern = this.SavePattern;
            clone.ListContent = this.ListContent;
            clone.Help = this.Help;
            clone.Version = this.Version;

            return clone;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion ICloneable
    }
}