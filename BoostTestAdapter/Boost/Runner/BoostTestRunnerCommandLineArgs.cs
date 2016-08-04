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

    public class ExecutionPath
    {
        public string TestName { get; set; }
        public uint PathNumber { get; set; }

        public override string ToString()
        {
            return this.TestName + ':' + this.PathNumber;
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

        private const string TestSeparator = ",";

        private const char ArgSeparator = ' ';
        private const char ArgValueSeparator = '=';

        private const char RedirectionOperator = '>';
        private const string ErrRedirectionOperator = "2>";

        private const string Yes = "yes";
        private const string No = "no";

        #endregion Constants

        #region Members

        private string _reportFile = null;
        private string _logFile = null;
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

            this.LogFormat = OutputFormat.Default;
            this.LogLevel = LogLevel.Default;

            this.ReportFormat = OutputFormat.Default;
            this.ReportLevel = ReportLevel.Default;

            this.DetectMemoryLeaks = 1;

            this.ShowProgress = false;
            this.BuildInfo = false;
            this.AutoStartDebug = "no";
            this.CatchSystemErrors = true;
            this.ColorOutput = false;
            this.ResultCode = true;
            this.Random = 0;
            this.UseAltStack = true;
            this.DetectFPExceptions = false;
            this.SavePattern = false;
            this.ListContent = null;
            this.Help = false;

            this.Environment = new Dictionary<string, string>();
        }

        #endregion Constructors

        /// <summary>
        /// Specify the process' working directory
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Specifies the process's environment
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
        /// Path (relative to the WorkingDirectory) to the log file which will host Boost Test output.
        /// </summary>
        public string LogFile
        {
            get
            {
                return GetPath(this._logFile);
            }

            set
            {
                this._logFile = value;
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
        /// Path (relative to the WorkingDirectory) to the report file which will host Boost Test report output.
        /// </summary>
        public string ReportFile
        {
            get
            {
                return GetPath(this._reportFile);
            }

            set
            {
                this._reportFile = value;
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
        /// Flag which displays Boost UTF test information in standard out.
        /// </summary>
        public bool BuildInfo { get; set; }

        /// <summary>
        /// Determines whether system errors should be caught.
        /// </summary>
        public bool CatchSystemErrors { get; set; }

        /// <summary>
        /// States whether standard output text is colour coded.
        /// </summary>
        /// <remarks>Introduced in Boost 1.59 / Boost Test 3</remarks>
        public bool ColorOutput { get; set; }

        /// <summary>
        /// Determines the result code the Boost UTF uses on exit.
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
        /// Instructs the Boost UTF to break on floating-point exceptions.
        /// </summary>
        public bool DetectFPExceptions { get; set; }

        /// <summary>
        /// Useful for test modules relying on boost::test_tools::output_test_stream. True implies 'save the pattern file'; false implies 'match against a previously stored pattern file'.
        /// </summary>
        /// <remarks>Introduced in Boost 1.59 / Boost Test 3</remarks>
        public bool SavePattern { get; set; }

        /// <summary>
        /// The Boost UTF lists all tests which are to be executed without actually executing the tests.
        /// </summary>
        /// <remarks>Introduced in Boost 1.59 / Boost Test 3</remarks>
        public ListContentFormat? ListContent { get; set; }

        /// <summary>
        /// Help output.
        /// </summary>
        public bool Help { get; set; }

        /// <summary>
        /// Path (relative to the WorkingDirectory) to the report file which will host the standard output content.
        /// </summary>
        public string StandardOutFile
        {
            get
            {
                return GetPath(this._stdOutFile);
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
                return GetPath(this._stdErrFile);
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

                // return immediately since Boost UTF should ignore the rest of the arguments
                return AppendRedirection(args).ToString().TrimEnd();
            }

            // --list_content
            if (this.ListContent != null)
            {
                AddArgument(ListContentArg, ListContentFormatToString(this.ListContent.Value), args);

                // return immediately since Boost UTF should ignore the rest of the arguments
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

            // --catch_system_errors=no
            if (!this.CatchSystemErrors)
            {
                AddArgument(CatchSystemErrorsArg, No, args);
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
            if (!string.IsNullOrEmpty(this._logFile))
            {
                AddArgument(LogSinkArg, this.LogFile, args);
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
            if (!string.IsNullOrEmpty(this._reportFile))
            {
                AddArgument(ReportSinkArg, this.ReportFile, args);
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
        /// Returns a rooted path for the provided one.
        /// </summary>
        /// <param name="path">The path to root.</param>
        /// <returns>The rooted path.</returns>
        protected string GetPath(string path)
        {
            if (!string.IsNullOrEmpty(this.WorkingDirectory) && !string.IsNullOrEmpty(path) && !Path.IsPathRooted(path))
            {
                return Path.Combine(this.WorkingDirectory, path);
            }

            return path;
        }
        

        /// <summary>
        /// Provides a (valid) string representation of the provided OutputFormat.
        /// </summary>
        /// <param name="value">The value to serialize to string.</param>
        /// <returns>A (valid) string representation of the provided OutputFormat.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static string OutputFormatToString(OutputFormat value)
        {
            // serge: boost 1.60 requires uppercase input
            return value.ToString();    //.ToLowerInvariant();
        }

        /// <summary>
        /// Provides a (valid) string representation of the provided LogLevel.
        /// </summary>
        /// <param name="value">The value to serialize to string.</param>
        /// <returns>A (valid) string representation of the provided LogLevel.</returns>
        private static string LogLevelToString(LogLevel value)
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
            clone._logFile = this._logFile;

            clone.ReportFormat = this.ReportFormat;
            clone.ReportLevel = this.ReportLevel;
            clone._reportFile = this._reportFile;

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

            return clone;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion ICloneable
    }
}