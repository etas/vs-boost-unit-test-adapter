// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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
    /// Aggregates all possible command line options made available by the Boost Test framework.
    /// Reference: http://www.boost.org/doc/libs/1_43_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html
    /// </summary>
    public class BoostTestRunnerCommandLineArgs
    {
        #region Constants

        private const string RunTestArg = "--run_test";

        private const string LogFormatArg = "--log_format";
        private const string LogLevelArg = "--log_level";
        private const string LogSinkArg = "--log_sink";

        private const string ReportFormatArg = "--report_format";
        private const string ReportLevelArg = "--report_level";
        private const string ReportSinkArg = "--report_sink";

        private const string DetectMemoryLeakArg = "--detect_memory_leak";

        private const string TestSeparator = ",";

        private const char ArgSeparator = ' ';
        private const char ArgValueSeparator = '=';

        private const char RedirectionOperator = '>';
        private const string ErrRedirectionOperator = "2>";

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
        }

        /// <summary>
        /// Copy Constructor. Creates a shallow copy of the provided instance.
        /// </summary>
        /// <param name="args">The instance to be copied.</param>
        protected BoostTestRunnerCommandLineArgs(BoostTestRunnerCommandLineArgs args)
        {
            Utility.Code.Require(args, "args");

            this.WorkingDirectory = args.WorkingDirectory;

            // Shallow copy
            this.Tests = args.Tests;

            this.LogFormat = args.LogFormat;
            this.LogLevel = args.LogLevel;
            this.LogFile = args.LogFile;

            this.ReportFormat = args.ReportFormat;
            this.ReportLevel = args.ReportLevel;
            this.ReportFile = args.ReportFile;

            this.DetectMemoryLeaks = args.DetectMemoryLeaks;

            this.StandardOutFile = args.StandardOutFile;
            this.StandardErrorFile = args.StandardErrorFile;
        }

        #endregion Constructors

        /// <summary>
        /// Specify the process' working directory
        /// </summary>
        public string WorkingDirectory { get; set; }

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

            // --run_tests=a,b,c
            if (this.Tests.Count > 0)
            {
                AddArgument(RunTestArg, string.Join(TestSeparator, this.Tests), args);
            }

            if (this.LogFormat != OutputFormat.Default)
            {
                // --log_format=xml
                AddArgument(LogFormatArg, OutputFormatToString(this.LogFormat), args);
            }

            if (this.LogLevel != LogLevel.Default)
            {
                // --log_level=test_suite
                AddArgument(LogLevelArg, LogLevelToString(this.LogLevel), args);
            }

            // --log_sink=log.xml
            if (!string.IsNullOrEmpty(this._logFile))
            {
                AddArgument(LogSinkArg, this._logFile, args);
            }

            if (this.ReportFormat != OutputFormat.Default)
            {
                // --report_format=xml
                AddArgument(ReportFormatArg, OutputFormatToString(this.ReportFormat), args);
            }

            if (this.ReportLevel != ReportLevel.Default)
            {
                // --report_level=detailed
                AddArgument(ReportLevelArg, ReportLevelToString(this.ReportLevel), args);
            }

            // --report_sink=report.xml
            if (!string.IsNullOrEmpty(this._reportFile))
            {
                AddArgument(ReportSinkArg, this._reportFile, args);
            }

            if (this.DetectMemoryLeaks != 1)
            {
                // --detect_memory_leak
                AddArgument(DetectMemoryLeakArg, this.DetectMemoryLeaks.ToString(CultureInfo.InvariantCulture), args);
            }

            // > std.out
            if (!string.IsNullOrEmpty(this._stdOutFile))
            {
                args.Append(RedirectionOperator).Append(ArgSeparator).Append(Quote(this._stdOutFile)).Append(ArgSeparator);
            }

            // 2> std.err
            if (!string.IsNullOrEmpty(this._stdErrFile))
            {
                args.Append(ErrRedirectionOperator).Append(ArgSeparator).Append(Quote(this._stdErrFile));
            }

            return args.ToString().TrimEnd();
        }

        /// <summary>
        /// Returns a rooted path for the provided one.
        /// </summary>
        /// <param name="path">The path to root.</param>
        /// <returns>The rooted path.</returns>
        protected string GetPath(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Path.IsPathRooted(path))
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
            return value.ToString().ToLowerInvariant();
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
    }
}