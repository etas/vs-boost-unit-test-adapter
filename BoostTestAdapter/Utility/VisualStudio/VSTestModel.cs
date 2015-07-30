using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoostTestAdapter.Boost.Results;
using BoostTestAdapter.Boost.Results.LogEntryTypes;
using BoostTestAdapter.Boost.Test;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using VSTestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;
using VSTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace BoostTestAdapter.Utility.VisualStudio
{
    /// <summary>
    /// Static class hosting utility methods related to the
    /// Visual Studio Test object model.
    /// </summary>
    public static class VSTestModel
    {
        /// <summary>
        /// TestSuite trait name
        /// </summary>
        public static string TestSuiteTrait
        {
            get
            {
                return "TestSuite";
            }
        }

        /// <summary>
        /// Converts a Boost.Test.Result.TestResult model into an equivalent
        /// Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult model.
        /// </summary>
        /// <param name="result">The Boost.Test.Result.TestResult model to convert.</param>
        /// <param name="test">The Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase model which is related to the result.</param>
        /// <returns>The Boost.Test.Result.TestResult model converted into its Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult counterpart.</returns>
        public static VSTestResult AsVSTestResult(this BoostTestAdapter.Boost.Results.TestResult result, VSTestCase test)
        {
            Utility.Code.Require(result, "result");
            Utility.Code.Require(test, "test");

            VSTestResult vsResult = new VSTestResult(test);

            vsResult.ComputerName = Environment.MachineName;

            vsResult.Outcome = GetTestOutcome(result.Result);

            // Boost.Test.Result.TestResult.Duration is in microseconds
            vsResult.Duration = TimeSpan.FromMilliseconds(result.Duration / 1000);

            if (result.LogEntries.Count > 0)
            {
                foreach (TestResultMessage message in GetTestMessages(result))
                {
                    vsResult.Messages.Add(message);
                }

                // Test using the TestOutcome type since elements from the
                // Boost Result type may be collapsed into a particular value
                if (vsResult.Outcome == VSTestOutcome.Failed)
                {
                    LogEntry error = GetLastError(result);

                    if (error != null)
                    {
                        vsResult.ErrorMessage = GetErrorMessage(result);
                        vsResult.ErrorStackTrace = ((error.Source == null) ? null : error.Source.ToString());
                    }
                }
            }

            return vsResult;
        }

        /// <summary>
        /// Converts a Boost.Test.Result.Result enumeration into an equivalent
        /// Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.
        /// </summary>
        /// <param name="result">The Boost.Test.Result.Result value to convert.</param>
        /// <returns>The Boost.Test.Result.Result enumeration converted into Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.</returns>
        private static VSTestOutcome GetTestOutcome(TestResultType result)
        {
            switch (result)
            {
                case TestResultType.Passed: return VSTestOutcome.Passed;
                case TestResultType.Skipped: return VSTestOutcome.Skipped;

                case TestResultType.Failed:
                case TestResultType.Aborted:
                default: return VSTestOutcome.Failed;
            }
        }

        /// <summary>
        /// Converts the log entries stored within the provided test result into equivalent
        /// Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResultMessages
        /// </summary>
        /// <param name="result">The Boost.Test.Result.TestResult whose LogEntries are to be converted.</param>
        /// <returns>An enumeration of TestResultMessage equivalent to the Boost log entries stored within the provided TestResult.</returns>
        private static IEnumerable<TestResultMessage> GetTestMessages(BoostTestAdapter.Boost.Results.TestResult result)
        {
            foreach (LogEntry entry in result.LogEntries)
            {
                string category = null;

                if (
                    (entry is LogEntryInfo) ||
                    (entry is LogEntryMessage) ||
                    (entry is LogEntryStandardOutputMessage)
                )
                {
                    category = TestResultMessage.StandardOutCategory;
                }
                else if (
                    (entry is LogEntryWarning) ||
                    (entry is LogEntryError) ||
                    (entry is LogEntryFatalError) ||
                    (entry is LogEntryMemoryLeak) ||
                    (entry is LogEntryException) ||
                    (entry is LogEntryStandardErrorMessage)
                )
                {
                    category = TestResultMessage.StandardErrorCategory;
                }
                else
                {
                    // Skip unknown message types
                    continue;
                }

                yield return new TestResultMessage(category, GetTestResultMessageText(result.Unit, entry));
            }
        }

        /// <summary>
        /// Given a log entry and its respective test unit, retuns a string
        /// formatted similar to the compiler_log_formatter.ipp in the Boost Test framework.
        /// </summary>
        /// <param name="unit">The test unit related to this log entry</param>
        /// <param name="entry">The log entry</param>
        /// <returns>A string message using a similar format as specified within compiler_log_formatter.ipp in the Boost Test framework</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static string GetTestResultMessageText(TestUnit unit, LogEntry entry)
        {
            if ((entry is LogEntryStandardOutputMessage) || (entry is LogEntryStandardErrorMessage))
            {
                return entry.Detail.TrimEnd() + Environment.NewLine;
            }

            StringBuilder sb = new StringBuilder();

            if (entry.Source != null)
            {
                AppendSourceInfo(entry.Source, sb);
            }

            sb.Append(entry.ToString().ToLowerInvariant()).
                Append(" in \"").
                Append(unit.Name).
                Append("\"");

            LogEntryMemoryLeak memoryLeak = entry as LogEntryMemoryLeak;
            if (memoryLeak == null)
            {
                sb.Append(": ").Append(entry.Detail.TrimEnd());
            }

            LogEntryException exception = entry as LogEntryException;
            if (exception != null)
            {
                if (exception.LastCheckpoint != null)
                {
                    sb.Append(Environment.NewLine);
                    AppendSourceInfo(exception.LastCheckpoint, sb);
                    sb.Append("last checkpoint: ").Append(exception.CheckpointDetail);
                }
            }

            if (memoryLeak != null)
            {
                if ((memoryLeak.LeakSourceFilePath != null) && (memoryLeak.LeakSourceFileName != null))
                {
                    sb.Append("source file path leak detected at :").
                        Append(memoryLeak.LeakSourceFilePath).
                        Append(memoryLeak.LeakSourceFileName);
                }

                if (memoryLeak.LeakLineNumber != null)
                {
                    sb.Append(", ").
                        Append("Line number: ").
                        Append(memoryLeak.LeakLineNumber);
                }

                sb.Append(", ").
                    Append("Memory allocation number: ").
                    Append(memoryLeak.LeakMemoryAllocationNumber);

                sb.Append(", ").
                    Append("Leak size: ").
                    Append(memoryLeak.LeakSizeInBytes).
                    Append(" byte");
                
                if (memoryLeak.LeakSizeInBytes > 0)
                {
                     sb.Append('s');
                }

                sb.Append(Environment.NewLine).
                    Append(memoryLeak.LeakLeakedDataContents);
            }

            // Append NewLine so that log entries are listed one per line
            return sb.Append(Environment.NewLine).ToString();
        }

        /// <summary>
        /// Compresses a message so that it is suitable for the UI.
        /// </summary>
        /// <param name="result">The erroneous LogEntry whose message is to be displayed.</param>
        /// <returns>A compressed message suitable for UI.</returns>
        private static string GetErrorMessage(BoostTestAdapter.Boost.Results.TestResult result)
        {
            StringBuilder sb = new StringBuilder();

            foreach (LogEntry error in GetErrors(result))
            {
                sb.Append(error.Detail).Append(Environment.NewLine);
            }

            // Remove redundant NewLine at the end
            sb.Remove((sb.Length - Environment.NewLine.Length), Environment.NewLine.Length);

            return sb.ToString();
        }

        /// <summary>
        /// Appends the SourceInfo instance information to the provided StringBuilder.
        /// </summary>
        /// <param name="info">The SourceInfo instance to stringify.</param>
        /// <param name="sb">The StringBuilder which will host the result.</param>
        /// <returns>sb</returns>
        private static StringBuilder AppendSourceInfo(SourceFileInfo info, StringBuilder sb)
        {
            sb.Append((string.IsNullOrEmpty(info.File) ? "unknown location" : info.File));
            if (info.LineNumber > -1)
            {
                sb.Append('(').Append(info.LineNumber).Append(')');
            }

            sb.Append(": ");

            return sb;
        }

        /// <summary>
        /// Given a TestResult returns the last error type log entry.
        /// </summary>
        /// <param name="result">The TestResult which hosts the necessary log entries</param>
        /// <returns>The last error type log entry or null if none are available.</returns>
        private static LogEntry GetLastError(BoostTestAdapter.Boost.Results.TestResult result)
        {
            // Select the last error issued within a Boost Test report
            return GetErrors(result).LastOrDefault();
        }

        /// <summary>
        /// Enumerates all log entries which are deemed to be an error (i.e. Warning, Error, Fatal Error and Exception).
        /// </summary>
        /// <param name="result">The TestResult which hosts the log entries.</param>
        /// <returns>An enumeration of error flagging log entries.</returns>
        private static IEnumerable<LogEntry> GetErrors(BoostTestAdapter.Boost.Results.TestResult result)
        {
            IEnumerable<LogEntry> errors = result.LogEntries.Where((e) =>
                                                    (e is LogEntryWarning) ||
                                                    (e is LogEntryError) ||
                                                    (e is LogEntryFatalError) ||
                                                    (e is LogEntryException)
                                               );

            // Only provide a single memory leak error if the test succeeded successfully (i.e. all asserts passed)
            return (errors.Any() ? errors : result.LogEntries.Where((e) => (e is LogEntryMemoryLeak)).Take(1));
        }
    }
}