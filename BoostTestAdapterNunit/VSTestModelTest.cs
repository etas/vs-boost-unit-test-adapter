using System;
using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter;
using BoostTestAdapter.Boost.Results;
using BoostTestAdapter.Boost.Results.LogEntryTypes;
using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using BoostTestResult = BoostTestAdapter.Boost.Results.TestResult;
using TestCase = BoostTestAdapter.Boost.Test.TestCase;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using VSTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class VSTestModelTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.TestCase = new VSTestCase(DefaultTestName, ExecutorUri, DefaultSource);
        }

        #endregion Test Setup/Teardown

        #region Test Data

        private static readonly Uri ExecutorUri = BoostTestExecutor.ExecutorUri;
        private const string DefaultTestName = "suite/test";
        private const string DefaultSource = "void";

        private VSTestCase TestCase { get; set; }

        #endregion Test Data
        
        #region Helper Classes

        /// <summary>
        /// BoostTestResult Builder. Provides a fluent-api interface to easily express BoostTestResult instances.
        /// </summary>
        private class BoostTestResultBuilder
        {
            public BoostTestResultBuilder()
            {
                this.Logs = new List<LogEntry>();
            }

            #region Properties

            private TestResultType ResultType { get; set; }
            private uint TimeDuration { get; set; }
            private IList<LogEntry> Logs { get; set; }
            private TestUnit Unit { get; set; }

            #endregion Properties

            /// <summary>
            /// Determine the Visual Studio test case for which this BoostTestResult is associated with.
            /// </summary>
            /// <param name="test">The Visual Studio test case to associate with the generated result</param>
            /// <returns>this</returns>
            public BoostTestResultBuilder For(VSTestCase test)
            {
                TestSuite parent = new TestSuite("Master Test Suite");

                string[] fragments = test.FullyQualifiedName.Split('/');
                for (int i = 0; i < (fragments.Length - 1); ++i)
                {
                    parent = new TestSuite(fragments[i], parent);
                }

                return For(new TestCase(fragments[(fragments.Length - 1)], parent));
            }

            /// <summary>
            /// Determine the Boost.Test.TestUnit test case for which this BoostTestResult is associated with.
            /// </summary>
            /// <param name="unit">The Boost.Test.TestUnit test case to associate with the generated result</param>
            /// <returns>this</returns>
            public BoostTestResultBuilder For(TestUnit unit)
            {
                this.Unit = unit;
                return this;
            }

            /// <summary>
            /// States that the test case passed.
            /// </summary>
            /// <returns>this</returns>
            public BoostTestResultBuilder Passed()
            {
                return Result(TestResultType.Passed);
            }

            /// <summary>
            /// States that the test case was aborted during execution.
            /// </summary>
            /// <returns>this</returns>
            public BoostTestResultBuilder Aborted()
            {
                return Result(TestResultType.Aborted);
            }

            /// <summary>
            /// States that the test case failed.
            /// </summary>
            /// <returns>this</returns>
            public BoostTestResultBuilder Failed()
            {
                return Result(TestResultType.Failed);
            }

            /// <summary>
            /// States that the test case was skipped by the testing framework.
            /// </summary>
            /// <returns>this</returns>
            public BoostTestResultBuilder Skipped()
            {
                return Result(TestResultType.Skipped);
            }

            /// <summary>
            /// States that the test case result.
            /// </summary>
            /// <param name="result">The test case execution result</param>
            /// <returns>this</returns>
            public BoostTestResultBuilder Result(TestResultType result)
            {
                this.ResultType = result;
                return this;
            }

            /// <summary>
            /// The duration of the test case execution in microseconds.
            /// </summary>
            /// <param name="time">Test case execution duration in microseconds</param>
            /// <returns>this</returns>
            public BoostTestResultBuilder Duration(uint time)
            {
                this.TimeDuration = time;
                return this;
            }

            /// <summary>
            /// Registers a log entry.
            /// </summary>
            /// <param name="entry">A log entry generated during test execution</param>
            /// <returns>this</returns>
            public BoostTestResultBuilder Log(LogEntry entry)
            {
                this.Logs.Add(entry);
                return this;
            }

            /// <summary>
            /// Generates a BoostTestResult instance from the contained configuration.
            /// </summary>
            /// <returns>A BoostTestResult instance based on the pre-set configuration</returns>
            public BoostTestResult Build()
            {
                BoostTestResult result = new BoostTestResult(null);

                result.Result = this.ResultType;
                result.Unit = this.Unit;
                result.Duration = this.TimeDuration;
                
                foreach (LogEntry entry in this.Logs)
                {
                    result.LogEntries.Add(entry);
                }

                return result;
            }
        }

        #endregion Helper Classes

        #region Helper Methods

        /// <summary>
        /// Asserts general Visual Studio TestResult properties for the default TestCase.
        /// </summary>
        /// <param name="result">The Visual Studio TestResult to test</param>
        private void AssertVSTestModelProperties(VSTestResult result)
        {
            AssertVSTestModelProperties(result, this.TestCase);
        }

        /// <summary>
        /// Asserts general Visual Studio TestResult properties for the provided TestCase.
        /// </summary>
        /// <param name="result">The Visual Studio TestResult to test</param>
        /// <param name="test">The Visual Studio TestCase for which the result is based on</param>
        private void AssertVSTestModelProperties(VSTestResult result, VSTestCase test)
        {
            Assert.That(result.TestCase, Is.EqualTo(test));
            Assert.That(result.ComputerName, Is.EqualTo(Environment.MachineName));
        }

        /// <summary>
        /// Asserts that a log entry resulting in a test failure is correctly noted
        /// in the Visual Studio TestResult.
        /// </summary>
        /// <param name="result">The Visual Studio TestResult to test</param>
        /// <param name="entry">The log entry detailing a test case error</param>
        private void AssertVsTestModelError(VSTestResult result, LogEntry entry)
        {
            Assert.That(result.ErrorMessage, Is.EqualTo(entry.Detail));
            Assert.That(result.ErrorStackTrace, Is.EqualTo(entry.Source.ToString()));
        }

        /// <summary>
        /// Converts from microseconds to a TimeSpan
        /// </summary>
        /// <param name="value">Duration in microseconds</param>
        /// <returns>A TimeSpan describing the microsecond duration</returns>
        private TimeSpan Microseconds(int value)
        {
            return TimeSpan.FromMilliseconds(Math.Truncate(value / 1000F));
        }
        
        /// <summary>
        /// Given a LogEntry instance, identifies the respective Visual Studio
        /// TestResult message category.
        /// </summary>
        /// <param name="entry">The LogEntry instance to test</param>
        /// <returns>The respective Visual Studio Message category for the provided LogEntry</returns>
        private string GetCategory(LogEntry entry)
        {
            BoostTestResult testCaseResult = new BoostTestResultBuilder().
                   For(this.TestCase).
                   Log(entry).
                   Build();

            VSTestResult result = testCaseResult.AsVSTestResult(this.TestCase);

            return result.Messages.First().Category;
        }

        #endregion Helper Methods

        #region Tests

        /// <summary>
        /// Boost Test Case passed
        /// 
        /// Test aims:
        ///     - Ensure that Boost TestCases which pass are identified accordingly in Visual Studio TestResults.
        /// </summary>
        [Test]
        public void ConvertPassToVSTestResult()
        {
            BoostTestResult testCaseResult = new BoostTestResultBuilder().
                For(this.TestCase).
                Passed().
                Duration(1000).
                Log(new LogEntryMessage("BOOST_MESSAGE output")).
                Build();

            VSTestResult result = testCaseResult.AsVSTestResult(this.TestCase);

            AssertVSTestModelProperties(result);

            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Passed));
            Assert.That(result.Duration, Is.EqualTo(Microseconds(1000)));

            Assert.That(result.Messages.Count, Is.EqualTo(1));

            TestResultMessage message = result.Messages.First();
            Assert.That(message.Category, Is.EqualTo(TestResultMessage.StandardOutCategory));
        }

        /// <summary>
        /// Boost Test Case failed
        /// 
        /// Test aims:
        ///     - Ensure that Boost TestCases which fail are identified accordingly in Visual Studio TestResults.
        /// </summary>
        [Test]
        public void ConvertFailToVSTestResult()
        {
            LogEntryError error = new LogEntryError("Error: 1 != 2", new SourceFileInfo("file.cpp", 10));

            BoostTestResult testCaseResult = new BoostTestResultBuilder().
                For(this.TestCase).
                Failed().
                Duration(2500).
                Log(error).
                Build();

            VSTestResult result = testCaseResult.AsVSTestResult(this.TestCase);

            AssertVSTestModelProperties(result);

            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Failed));
            Assert.That(result.Duration, Is.EqualTo(Microseconds(2500)));

            AssertVsTestModelError(result, error);

            Assert.That(result.Messages.Count, Is.EqualTo(1));

            TestResultMessage message = result.Messages.First();
            Assert.That(message.Category, Is.EqualTo(TestResultMessage.StandardErrorCategory));
        }

        /// <summary>
        /// Boost Test Case skipped
        /// 
        /// Test aims:
        ///     - Ensure that Boost TestCases which are skipped are identified accordingly in Visual Studio TestResults.
        /// </summary>
        [Test]
        public void ConvertSkipToVSTestResult()
        {
            BoostTestResult testCaseResult = new BoostTestResultBuilder().
                For(this.TestCase).
                Skipped().
                Build();

            VSTestResult result = testCaseResult.AsVSTestResult(this.TestCase);

            AssertVSTestModelProperties(result);

            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Skipped));
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.Zero));

            Assert.That(result.Messages.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Boost Test Case exception
        /// 
        /// Test aims:
        ///     - Ensure that Boost TestCases which throw an exception are identified accordingly in Visual Studio TestResults.
        /// </summary>
        [Test]
        public void ConvertExceptionToVSTestResult()
        {
            LogEntryException exception = new LogEntryException("C string: some error", new SourceFileInfo("unknown location", 0));
            exception.LastCheckpoint = new SourceFileInfo("boostunittestsample.cpp", 13);
            exception.CheckpointDetail = "Going to throw an exception";

            BoostTestResult testCaseResult = new BoostTestResultBuilder().
                For(this.TestCase).
                Aborted().
                Duration(0).
                Log(exception).
                Build();

            VSTestResult result = testCaseResult.AsVSTestResult(this.TestCase);

            AssertVSTestModelProperties(result);

            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Failed));
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromTicks(0)));

            AssertVsTestModelError(result, exception);

            Assert.That(result.Messages.Count, Is.EqualTo(1));

            TestResultMessage message = result.Messages.First();
            Assert.That(message.Category, Is.EqualTo(TestResultMessage.StandardErrorCategory));
        }


        /// <summary>
        /// Boost Test Case Memory Leak
        /// 
        /// Test aims:
        ///     - Ensure that Boost TestCases which leak memory are identified accordingly in Visual Studio TestResults.
        /// </summary>
        [Test]
        public void ConvertMemoryLeakToVSTestResult()
        {
            LogEntryMemoryLeak leak = new LogEntryMemoryLeak();

            leak.LeakLineNumber = 32;
            leak.LeakMemoryAllocationNumber = 836;
            leak.LeakLeakedDataContents = " Data: <`-  Leak...     > 60 2D BD 00 4C 65 61 6B 2E 2E 2E 00 CD CD CD CD ";

            leak.LeakSourceFilePath = @"C:\boostunittestsample.cpp";
            leak.LeakSourceFileName = "boostunittestsample.cpp";

            BoostTestResult testCaseResult = new BoostTestResultBuilder().
                For(this.TestCase).
                Passed().
                Duration(1000).
                Log(leak).
                Build();

            VSTestResult result = testCaseResult.AsVSTestResult(this.TestCase);

            AssertVSTestModelProperties(result);

            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Passed));
            Assert.That(result.Duration, Is.EqualTo(Microseconds(1000)));

            Assert.That(result.Messages.Count, Is.EqualTo(1));

            TestResultMessage message = result.Messages.First();
            Assert.That(message.Category, Is.EqualTo(TestResultMessage.StandardErrorCategory));
        }

        /// <summary>
        /// Log entries identification in Visual Studio TestResults
        /// 
        /// Test aims:
        ///     - Ensure that Boost log entries are categorized as expected when converted into Visual Studio TestResult Messages.
        /// </summary>
        [Test]
        public void TestLogEntryCategory()
        {
            // Standard Output
            Assert.That(GetCategory(new LogEntryInfo("Info")), Is.EqualTo(TestResultMessage.StandardOutCategory));
            Assert.That(GetCategory(new LogEntryMessage("Message")), Is.EqualTo(TestResultMessage.StandardOutCategory));
            Assert.That(GetCategory(new LogEntryStandardOutputMessage("StdOut")), Is.EqualTo(TestResultMessage.StandardOutCategory));

            // Standard Error
            Assert.That(GetCategory(new LogEntryWarning("Warning")), Is.EqualTo(TestResultMessage.StandardErrorCategory));
            Assert.That(GetCategory(new LogEntryError("Error")), Is.EqualTo(TestResultMessage.StandardErrorCategory));
            Assert.That(GetCategory(new LogEntryFatalError("FatalError")), Is.EqualTo(TestResultMessage.StandardErrorCategory));
            Assert.That(GetCategory(new LogEntryException("Exception")), Is.EqualTo(TestResultMessage.StandardErrorCategory));
            Assert.That(GetCategory(new LogEntryMemoryLeak()), Is.EqualTo(TestResultMessage.StandardErrorCategory));
            Assert.That(GetCategory(new LogEntryStandardErrorMessage("StdErr")), Is.EqualTo(TestResultMessage.StandardErrorCategory));
        }

        #endregion Tests
    }
}
