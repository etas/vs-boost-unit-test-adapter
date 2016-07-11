// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BoostTestAdapter.Boost.Results;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.TestBatch;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using System.Runtime.InteropServices;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using VSTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace BoostTestAdapter
{
    /// <summary>
    /// Implementation of ITestExecutor interface for Boost Tests.
    /// </summary>
    [ExtensionUri(ExecutorUriString)]
    public class BoostTestExecutor : ITestExecutor
    {
        #region Constants

        public const string ExecutorUriString = "executor://BoostTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);

        // Error issued by Boost Test when a test cannot be executed.
        private const string TestNotFound = "Test setup error: no test cases matching filter";

        /// <summary>
        /// Static class aggregating constant file extensions.
        /// </summary>
        private static class FileExtensions
        {
            public const string LogFile = ".test.log.xml";
            public const string ReportFile = ".test.report.xml";
            public const string StdOutFile = ".test.stdout.log";
            public const string StdErrFile = ".test.stderr.log";
        }

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public BoostTestExecutor()
        {
            _testRunnerFactory = new DefaultBoostTestRunnerFactory();
            _boostTestDiscovererFactory = new BoostTestDiscovererFactory(_testRunnerFactory);
            _vsProvider = new DefaultVisualStudioInstanceProvider();

            _cancelled = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="testRunnerFactory">The IBoostTestRunnerFactory which is to be used</param>
        /// <param name="boostTestDiscovererFactory">The IBoostTestDiscovererFactory which is to be used</param>
        /// <param name="provider">The Visual Studio instance provider</param>
        public BoostTestExecutor(IBoostTestRunnerFactory testRunnerFactory, IBoostTestDiscovererFactory boostTestDiscovererFactory, IVisualStudioInstanceProvider provider)
        {
            _testRunnerFactory = testRunnerFactory;
            _boostTestDiscovererFactory = boostTestDiscovererFactory;
            _vsProvider = provider;

            _cancelled = false;
        }

        #endregion Constructors
        
        #region Member variables

        private volatile bool _cancelled;
        private readonly IBoostTestDiscovererFactory _boostTestDiscovererFactory;
        private readonly IBoostTestRunnerFactory _testRunnerFactory;

        /// <summary>
        /// The Visual Studio instance provider
        /// </summary>
        private readonly IVisualStudioInstanceProvider _vsProvider;


        #endregion Member variables
        
        /// <summary>

        /// <summary>
        /// Initialization routine for running tests
        /// </summary>
        /// <param name="logger">The logger which will be used to emit log messages</param>
        private void SetUp(IMessageLogger logger)
        {
#if DEBUG && LAUNCH_DEBUGGER
            System.Diagnostics.Debugger.Launch();
#endif

            _cancelled = false;
            Logger.Initialize(logger);
        }

        /// <summary>
        /// Termination/Cleanup routine for running tests
        /// </summary>
        private static void TearDown()
        {
            Logger.Shutdown();
        }

        /// <summary>
        /// Filters out any tests which are not intended to run
        /// </summary>
        /// <param name="settings">Adapter settings which determines test filtering</param>
        /// <param name="tests">The entire test corpus</param>
        /// <returns>A test corpus which contains only the test which are intended to run</returns>
        private static IEnumerable<TestCase> GetTestsToRun(BoostTestAdapterSettings settings, IEnumerable<TestCase> tests)
        {
            IEnumerable<TestCase> testsToRun = tests;

            if (!settings.RunDisabledTests)
            {
                testsToRun = tests.Where((test) =>
                {
                    foreach (var trait in test.Traits)
                    {
                        if ((trait.Name == VSTestModel.StatusTrait) && (trait.Value == VSTestModel.TestEnabled))
                        {
                            return true;
                        }
                    }

                    return false;
                });
            }

            return testsToRun;
        }

        #region ITestExecutor

        /// <summary>
        /// Execute the tests one by one. Run All.
        /// </summary>
        /// <param name="sources">Collection of test modules (exe/dll)</param>
        /// <param name="runContext">Solution properties</param>
        /// <param name="frameworkHandle">Unit test framework handle</param>
        /// <remarks>Entry point of the execution procedure whenever the user requests to run all the tests</remarks>
        public void RunTests(IEnumerable<string> sources,
            IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            Code.Require(sources, "sources");
            Code.Require(runContext, "runContext");
            Code.Require(frameworkHandle, "frameworkHandle");

            SetUp(frameworkHandle);

            Logger.Debug("RunSettings: {0}", runContext.RunSettings.SettingsXml);
            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(runContext);

            foreach (string source in sources)
            {
                if (_cancelled)
                {
                    break;
                }

                var discoverer = _boostTestDiscovererFactory.GetDiscoverer(source, settings);
                if (discoverer != null)
                {
                    try
                    {
                        DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

                        // NOTE IRunContext implements IDiscoveryContext
                        // NOTE IFrameworkHandle implements IMessageLogger

                        // Re-discover tests so that we could make use of the RunTests overload which takes an enumeration of test cases.
                        // This is necessary since we need to run tests one by one in order to have the test adapter remain responsive
                        // and have a list of tests over which we can generate test results for.
                        discoverer.DiscoverTests(new[] { source }, runContext, sink);


                        //The following ensures that only test cases that are not disabled are run when the user presses "Run all"
                        //This, however, can be overwritten by the .runsettings file supplied
                        IEnumerable<TestCase> testsToRun = GetTestsToRun(settings, sink.Tests);

                        // Batch tests into grouped runs based by source so that we avoid reloading symbols per test run
                        // Batching by source since this overload is called when 'Run All...' or equivalent is triggered
                        // NOTE For code-coverage speed is given preference over adapter responsiveness.
                        TestBatch.Strategy strategy = ((runContext.IsDataCollectionEnabled) ? TestBatch.Strategy.Source : settings.TestBatchStrategy);
                        
                        ITestBatchingStrategy batchStrategy = GetBatchStrategy(strategy, settings);
                        if (batchStrategy == null)
                        {
                            Logger.Error("No valid test batching strategy was found for {0}. Source skipped.", source);
                            continue;
                        }

                        IEnumerable<TestRun> batches = batchStrategy.BatchTests(testsToRun);

                        // Delegate to the RunBoostTests overload which takes an enumeration of test batches
                        RunBoostTests(batches, runContext, frameworkHandle);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception caught while running tests from {0} ({1})", source, ex.Message);
                    }
                }
            }

            TearDown();
        }

        /// <summary>
        /// Execute the tests one by one. Run Selected
        /// </summary>
        /// <param name="tests">Testcases object</param>
        /// <param name="runContext">Solution properties</param>
        /// <param name="frameworkHandle">Unit test framework handle</param>
        /// <remarks>Entry point of the execution procedure whenever the user requests to run one or a specific lists of tests</remarks>
        public void RunTests(IEnumerable<VSTestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Code.Require(tests, "tests");
            Code.Require(runContext, "runContext");
            Code.Require(frameworkHandle, "frameworkHandle");

            SetUp(frameworkHandle);
            
            Logger.Debug("RunSettings: {0}", runContext.RunSettings.SettingsXml);
            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(runContext);

            // Batch tests into grouped runs based on test source and test suite so that we minimize symbol reloading
            //
            // NOTE Required batching at test suite level since Boost Unit Test Framework command-line arguments only allow
            //      multiple test name specification for tests which reside in the same test suite
            //
            // NOTE For code-coverage speed is given preference over adapter responsiveness.
            TestBatch.Strategy strategy = ((runContext.IsDataCollectionEnabled) ? TestBatch.Strategy.TestSuite : settings.TestBatchStrategy);
            // Source strategy is invalid in such context since explicit tests are chosen. TestSuite is used instead.
            if (strategy == Strategy.Source)
            {
                strategy = Strategy.TestSuite;
            }

            ITestBatchingStrategy batchStrategy = GetBatchStrategy(strategy, settings);
            if (batchStrategy == null)
            {
                Logger.Error("No valid test batching strategy was found. Tests skipped.");
            }
            else
            {
                // NOTE Apply distinct to avoid duplicate test cases. Common issue when using BOOST_DATA_TEST_CASE.
                IEnumerable<TestRun> batches = batchStrategy.BatchTests(tests.Distinct(new TestCaseComparer()));
                RunBoostTests(batches, runContext, frameworkHandle);
            }

            TearDown();
        }

        /// <summary>
        /// Cancel the execution of tests
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }

        #endregion ITestExecutor

        #region Test Batching

        /// <summary>
        /// Provides a test batching strategy based on the provided arguments
        /// </summary>
        /// <param name="strategy">The base strategy to provide</param>
        /// <param name="settings">Adapter settings currently in use</param>
        /// <returns>An ITestBatchingStrategy instance or null if one cannot be provided</returns>
        private ITestBatchingStrategy GetBatchStrategy(TestBatch.Strategy strategy, BoostTestAdapterSettings settings)
        {
            TestBatch.CommandLineArgsBuilder argsBuilder = GetDefaultArguments;
            if (strategy != Strategy.TestCase)
            {
                argsBuilder = GetBatchedTestRunsArguments;
            }

            switch (strategy)
            {
                case Strategy.Source: return new SourceTestBatchStrategy(this._testRunnerFactory, settings, argsBuilder);
                case Strategy.TestSuite: return new TestSuiteTestBatchStrategy(this._testRunnerFactory, settings, argsBuilder);
                case Strategy.TestCase: return new IndividualTestBatchStrategy(this._testRunnerFactory, settings, argsBuilder);
            }

            return null;
        }

        #endregion Test Batching

        #region Helper methods

        /// <summary>
        /// Run tests one test at a time and update results back to framework.
        /// </summary>
        /// <param name="testBatches">List of test batches to run</param>
        /// <param name="runContext">Solution properties</param>
        /// <param name="frameworkHandle">Unit test framework handle</param>
        private void RunBoostTests(IEnumerable<TestRun> testBatches, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(runContext);

            foreach (TestRun batch in testBatches)
            {
                if (_cancelled)
                {
                    break;
                }

                DateTimeOffset start = new DateTimeOffset(DateTime.Now);

                try
                {
                    Logger.Info("{0}:   -> [{1}]", ((runContext.IsBeingDebugged) ? "Debugging" : "Executing"), string.Join(", ", batch.Tests));

                    using (TemporaryFile report = new TemporaryFile(batch.Arguments.ReportFile))
                    using (TemporaryFile log    = new TemporaryFile(batch.Arguments.LogFile))
                    using (TemporaryFile stdout = new TemporaryFile(batch.Arguments.StandardOutFile))
                    using (TemporaryFile stderr = new TemporaryFile(batch.Arguments.StandardErrorFile))
                    {
                        Logger.Debug("Working directory: {0}", batch.Arguments.WorkingDirectory ?? "(null)");
                        Logger.Debug("Report file      : {0}", batch.Arguments.ReportFile);
                        Logger.Debug("Log file         : {0}", batch.Arguments.LogFile);
                        Logger.Debug("StdOut file      : {0}", batch.Arguments.StandardOutFile ?? "(null)");
                        Logger.Debug("StdErr file      : {0}", batch.Arguments.StandardErrorFile ?? "(null)");

                        // Execute the tests
                        if (ExecuteTests(batch, runContext, frameworkHandle))
                        {
                            foreach (VSTestResult result in GenerateTestResults(batch, start, settings))
                            {
                                // Identify test result to Visual Studio Test framework
                                frameworkHandle.RecordResult(result);
                            }
                        }
                    }
                }
                catch (Boost.Runner.TimeoutException ex)
                {
                    foreach (VSTestCase testCase in batch.Tests)
                    {
                        VSTestResult testResult = GenerateTimeoutResult(testCase, ex);
                        testResult.StartTime = start;

                        frameworkHandle.RecordResult(testResult);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Exception caught while running test batch {0} [{1}] ({2})", batch.Source, string.Join(", ", batch.Tests), ex.Message);
                }
            }
        }
        
        /// <summary>
        /// Executes the provided test batch
        /// </summary>
        /// <param name="run">The test batch which will be executed.</param>
        /// <param name="runContext">The RunContext for this TestCase. Determines whether the test should be debugged or not.</param>
        /// <param name="frameworkHandle">The FrameworkHandle for this test execution instance.</param>
        /// <returns></returns>
        private static bool ExecuteTests(TestRun run, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (run.Runner != null)
            {
                if (runContext.IsBeingDebugged)
                {
                    run.Debug(frameworkHandle);
                }
                else
                {
                    run.Run();
                }
            }
            else
            {
                Logger.Error("No suitable executor found for [{0}].", string.Join(", ", run.Tests));
            }

            return run.Runner != null;
        }
        
        /// <summary>
        /// Retrieves and assigns parameters by resolving configurations from different possible resources
        /// </summary>
        /// <param name="source">The TestCases source</param>
        /// <param name="settings">The Boost Test adapter settings currently in use</param>
        /// <returns>A string for the default working directory</returns>
        private void GetDebugConfigurationProperties(string source, BoostTestAdapterSettings settings, BoostTestRunnerCommandLineArgs args)
        {
            try
            {
                args.SetWorkingEnvironment(source, settings, ((_vsProvider == null) ? null : _vsProvider.Instance));
            }
            catch (COMException ex)
            {
                Logger.Exception(ex, "Could not retrieve WorkingDirectory from Visual Studio Configuration-{0}", ex.Message);
            }
        }

        /// <summary>
        /// Factory function which returns an appropriate BoostTestRunnerCommandLineArgs structure
        /// </summary>
        /// <param name="source">The TestCases source</param>
        /// <param name="settings">The Boost Test adapter settings currently in use</param>
        /// <returns>A BoostTestRunnerCommandLineArgs structure for the provided source</returns>
        private BoostTestRunnerCommandLineArgs GetDefaultArguments(string source, BoostTestAdapterSettings settings)
        {
            BoostTestRunnerCommandLineArgs args = settings.CommandLineArgs.Clone();

            GetDebugConfigurationProperties(source, settings, args);
            
            // Specify log and report file information
            args.LogFormat = OutputFormat.XML;
            args.LogLevel = settings.LogLevel;
            args.LogFile = TestPathGenerator.Generate(source, FileExtensions.LogFile);

            args.ReportFormat = OutputFormat.XML;
            args.ReportLevel = ReportLevel.Detailed;
            args.ReportFile = TestPathGenerator.Generate(source, FileExtensions.ReportFile);
            
            args.StandardOutFile = ((settings.EnableStdOutRedirection) ? TestPathGenerator.Generate(source, FileExtensions.StdOutFile) : null);
            args.StandardErrorFile = ((settings.EnableStdErrRedirection) ? TestPathGenerator.Generate(source, FileExtensions.StdErrFile) : null);

            return args;
        }
        
        /// <summary>
        /// Factory function which returns an appropriate BoostTestRunnerCommandLineArgs structure for batched test runs
        /// </summary>
        /// <param name="source">The TestCases source</param>
        /// <param name="settings">The Boost Test adapter settings currently in use</param>
        /// <returns>A BoostTestRunnerCommandLineArgs structure for the provided source</returns>
        private BoostTestRunnerCommandLineArgs GetBatchedTestRunsArguments(string source, BoostTestAdapterSettings settings)
        {
            BoostTestRunnerCommandLineArgs args = GetDefaultArguments(source, settings);

            // Disable standard error/standard output capture
            args.StandardOutFile = null;
            args.StandardErrorFile = null;

            // Disable memory leak detection
            args.DetectMemoryLeaks = 0;

            return args;
        }

        /// <summary>
        /// Generates TestResults based on Boost Test result output.
        /// </summary>
        /// <param name="testRun">The tests which have been executed in the prior test run.</param>
        /// <param name="start">The test execution start time.</param>
        /// <param name="settings">boost test adapter settings</param>
        /// <returns>A Visual Studio TestResult related to the executed test.</returns>
        private static IEnumerable<VSTestResult> GenerateTestResults(TestRun testRun, DateTimeOffset start, BoostTestAdapterSettings settings)
        {
            return GenerateTestResults(testRun, start, DateTimeOffset.Now, settings);
        }

        /// <summary>
        /// Generates TestResults based on Boost Test result output.
        /// </summary>
        /// <param name="testRun">The tests which have been executed in the prior test run.</param>
        /// <param name="start">The test execution start time.</param>
        /// <param name="end">The test execution end time.</param>
        /// <param name="settings">boost test adapter settings</param>
        /// <returns>A Visual Studio TestResult related to the executed test.</returns>
        private static IEnumerable<VSTestResult> GenerateTestResults(TestRun testRun, DateTimeOffset start, DateTimeOffset end, BoostTestAdapterSettings settings)
        {
            TestResultCollection results = new TestResultCollection();

            try
            {
                results.Parse(testRun.Arguments, settings);
            }
            catch (XmlException)
            {
                string text = ((File.Exists(testRun.Arguments.ReportFile)) ? File.ReadAllText(testRun.Arguments.ReportFile) : string.Empty);

                if (text.Trim().StartsWith(TestNotFound, StringComparison.Ordinal))
                {
                    return testRun.Tests.Select(GenerateNotFoundResult);
                }
                else
                {
                    // Represent result parsing exception as a test fatal error
                    if (string.IsNullOrEmpty(text))
                    {
                        text = "Boost Test result file was not found or is empty.";
                    }
                    
                    return testRun.Tests.Select(test => {
                        Boost.Results.TestResult exception = new Boost.Results.TestResult(results);
                        
                        exception.Unit = Boost.Test.TestUnit.FromFullyQualifiedName(test.FullyQualifiedName);

                        // NOTE Divide by 10 to compensate for duration calculation described in VSTestResult.AsVSTestResult(this Boost.Results.TestResult, VSTestCase)
                        exception.Duration = ((ulong)(end - start).Ticks) / 10;

                        exception.Result = TestResultType.Failed;
                        exception.LogEntries.Add(new Boost.Results.LogEntryTypes.LogEntryFatalError(text));

                        return GenerateResult(test, exception, start, end);
                    });
                }
            }

            return testRun.Tests.
                Select(test =>
                {
                    // Locate the test result associated to the current test
                    Boost.Results.TestResult result = results[test.FullyQualifiedName];
                    return (result == null) ? null : GenerateResult(test, result, start, end);
                }).
                Where(result => (result != null));
        }

        private static VSTestResult GenerateResult(VSTestCase test, Boost.Results.TestResult result, DateTimeOffset start, DateTimeOffset end)
        {
            Code.Require(test, "test");
            Code.Require(result, "result");

            // Convert the Boost.Test.Result data structure into an equivalent Visual Studio model
            VSTestResult vsResult = result.AsVSTestResult(test);
            vsResult.StartTime = start;
            vsResult.EndTime = end;

            return vsResult;
        }

        /// <summary>
        /// Generates a default TestResult for a timeout exception.
        /// </summary>
        /// <param name="test">The test which failed due to a timeout.</param>
        /// <param name="ex">The exception related to this timeout.</param>
        /// <returns>A timed-out, failed TestResult related to the provided test.</returns>
        private static VSTestResult GenerateTimeoutResult(VSTestCase test, Boost.Runner.TimeoutException ex)
        {
            VSTestResult result = new VSTestResult(test);

            result.ComputerName = Environment.MachineName;

            result.Outcome = TestOutcome.Failed;
            result.Duration = TimeSpan.FromMilliseconds(ex.Timeout);
            result.ErrorMessage = "Timeout exceeded. Test ran for more than " + ex.Timeout + " ms.";

            if (!string.IsNullOrEmpty(test.CodeFilePath))
            {
                result.ErrorStackTrace = new SourceFileInfo(test.CodeFilePath, test.LineNumber).ToString();
            }

            return result;
        }

        /// <summary>
        /// Generates a default TestResult for a 'test not found' exception.
        /// </summary>
        /// <param name="test">The test which failed due to a timeout.</param>
        /// <returns>A timed-out, failed TestResult related to the provided test.</returns>
        private static VSTestResult GenerateNotFoundResult(VSTestCase test)
        {
            VSTestResult result = new VSTestResult(test);

            result.ComputerName = Environment.MachineName;

            result.Outcome = TestOutcome.Skipped;
            result.ErrorMessage = GetNotFoundErrorMessage(test);

            return result;
        }

        /// <summary>
        /// Provides a suitable message in case the provided test is not found.
        /// </summary>
        /// <param name="test">The test which was not found.</param>
        /// <returns>A suitable 'not-found' for the provided test case.</returns>
        private static string GetNotFoundErrorMessage(VSTestCase test)
        {
            if (test.FullyQualifiedName.Contains(' '))
            {
                return TestNotFound + " (Test name contains spaces)";
            }
            else if (test.FullyQualifiedName.Contains(','))
            {
                return TestNotFound + " (Test name contains commas)";
            }

            return TestNotFound;
        }

        #endregion Helper methods
    }
}