using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BoostTestAdapter.Boost.Results;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
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
            : this(new DefaultBoostTestDiscovererFactory(), new DefaultBoostTestRunnerFactory())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="discovererFactory">The ITestDiscovererFactory which is to be used</param>
        /// <param name="testRunnerFactory">The IBoostTestRunnerFactory which is to be used</param>
        public BoostTestExecutor(IBoostTestDiscovererFactory discovererFactory, IBoostTestRunnerFactory testRunnerFactory)
        {
            this.DiscovererFactory = discovererFactory;
            this.TestRunnerFactory = testRunnerFactory;

            this._cancelled = false;
        }

        #endregion Constructors

        #region Member variables

        private volatile bool _cancelled = false;

        #endregion Member variables

        #region Properties

        private IBoostTestDiscovererFactory DiscovererFactory { get; set; }

        private IBoostTestRunnerFactory TestRunnerFactory { get; set; }

        #endregion Properties

        #region Delegates

        private delegate BoostTestRunnerCommandLineArgs CommandLineArgsBuilder(string source, BoostTestAdapterSettings settings);

        #endregion Delegates

        /// <summary>
        /// Factory function which returns an appropriate ITestDiscoverer
        /// for the provided source or null if not applicable.
        /// </summary>
        /// <param name="source">The source module which requires test discovery</param>
        /// <returns>An IBoostTestDiscoverer valid for the provided source or null if none are available</returns>
        private IBoostTestDiscoverer GetTestDiscoverer(string source, BoostTestAdapterSettings settings)
        {
            Utility.Code.Require(settings, "settings");

            BoostTestDiscovererFactoryOptions options = new BoostTestDiscovererFactoryOptions();
            options.ExternalTestRunnerSettings = settings.ExternalTestRunner;

            return this.DiscovererFactory.GetTestDiscoverer(source, options);
        }

        /// <summary>
        /// Factory function which returns an appropriate IBoostTestRunner
        /// for the provided source or null if not applicable.
        /// </summary>
        /// <param name="testCase">The test for which to retrieve the IBoostTestRunner</param>
        /// <returns>An IBoostTestRunner valid for the provided source or null if none are available</returns>
        private IBoostTestRunner GetTestRunner(VSTestCase testCase, BoostTestAdapterSettings settings)
        {
            BoostTestRunnerFactoryOptions options = new BoostTestRunnerFactoryOptions();
            options.ExternalTestRunnerSettings = (settings == null) ? null : settings.ExternalTestRunner;

            IBoostTestRunner runner = this.TestRunnerFactory.GetRunner(testCase.Source, options);

            // Using null instance pattern to avoid null reference exceptions raised with use of Linq GroupBy statements
            return (runner == null) ? NullTestRunner.Instance : runner;
        }

        /// <summary>
        /// Initialization routine for running tests
        /// </summary>
        /// <param name="logger">The logger which will be used to emit log messages</param>
        private void SetUp(IMessageLogger logger)
        {
#if DEBUG && LAUNCH_DEBUGGER
            System.Diagnostics.Debugger.Launch();
#endif

            this._cancelled = false;
            Logger.Initialize(logger);
        }

        /// <summary>
        /// Termination/Cleanup routine for running tests
        /// </summary>
        private static void TearDown()
        {
            Logger.Shutdown();
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
            Utility.Code.Require(sources, "sources");
            Utility.Code.Require(runContext, "runContext");
            Utility.Code.Require(frameworkHandle, "frameworkHandle");

            SetUp(frameworkHandle);

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(runContext);

            foreach (string source in sources)
            {
                if (this._cancelled)
                {
                    break;
                }

                IBoostTestDiscoverer discoverer = GetTestDiscoverer(source, settings);

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
                        discoverer.DiscoverTests(new string[] { source }, runContext, frameworkHandle, sink);

                        IEnumerable<TestRun> batches = null;
                        if (runContext.IsDataCollectionEnabled)
                        {
                            // Batch tests into grouped runs based by source so that we avoid reloading symbols per test run
                            // NOTE For code-coverage speed is given preference over adapter responsiveness.
                            batches = BatchTestsPerSource(sink.Tests, settings, GetCodeCoverageArguments);
                        }
                        else
                        {
                            batches = BatchTestsIndividually(sink.Tests, settings, GetDefaultArguments);
                        }

                        // Delegate to the RunBoostTests overload which takes an enumeration of test batches
                        RunBoostTests(batches, runContext, frameworkHandle);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception caught while running tests from {0} ({1})", source, ex.Message);
                    }
                }
                else
                {
                    Logger.Error("No suitable discoverer found for {0}.", source);
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
            Utility.Code.Require(tests, "tests");
            Utility.Code.Require(runContext, "runContext");
            Utility.Code.Require(frameworkHandle, "frameworkHandle");

            SetUp(frameworkHandle);

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(runContext);

            IEnumerable<TestRun> batches = null;
            if (runContext.IsDataCollectionEnabled)
            {
                // Batch tests into grouped runs based on test source and test suite so that we minimize symbol reloading
                //
                // NOTE Required batching at test suite level since Boost Unit Test Framework command-line arguments only allow
                //      multiple test name specification for tests which reside in the same test suite
                //
                // NOTE For code-coverage speed is given preference over adapter responsiveness.
                batches = BatchTestsPerTestSuite(tests, settings, GetCodeCoverageArguments);
            }
            else
            {
                batches = BatchTestsIndividually(tests, settings, GetDefaultArguments);
            }

            RunBoostTests(batches, runContext, frameworkHandle);

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
        /// Produces test runs, one per test source
        /// </summary>
        /// <param name="tests">The tests to prepare in batches</param>
        /// <param name="settings">Adapter settings which are currently in use</param>
        /// <param name="argsBuilder">A builder which produces an appropriate BoostTestRunnerCommandLineArgs structure for a given test and settings pair</param>
        /// <returns>An enumeration of batched test runs, one per distinct test source</returns>
        private IEnumerable<TestRun> BatchTestsPerSource(IEnumerable<VSTestCase> tests, BoostTestAdapterSettings settings, CommandLineArgsBuilder argsBuilder)
        {
            BoostTestRunnerSettings adaptedSettings = settings.TestRunnerSettings.Clone();
            adaptedSettings.Timeout = -1;

            return tests.
                GroupBy((source) => GetTestRunner(source, settings), new BoostTestRunnerComparer()).
                Where((group) => (group.Key != NullTestRunner.Instance)).
                // Project IGrouping<IBoostTestRunner, VSTestCase> into TestRun instances
                Select(group =>
                {
                    BoostTestRunnerCommandLineArgs args = argsBuilder(group.Key.Source, settings);

                    // NOTE the --run_test command-line arg is left empty so that all tests are executed

                    return new TestRun(group.Key, group, args, adaptedSettings);
                });
        }

        /// <summary>
        /// Produces batched test runs grouped by source and test suite
        /// </summary>
        /// <param name="tests">The tests to prepare in batches</param>
        /// <param name="settings">Adapter settings which are currently in use</param>
        /// <param name="argsBuilder">A builder which produces an appropriate BoostTestRunnerCommandLineArgs structure for a given test and settings pair</param>
        /// <returns>An enumeration of groups of tests batched into test runs</returns>
        private IEnumerable<TestRun> BatchTestsPerTestSuite(IEnumerable<VSTestCase> tests, BoostTestAdapterSettings settings, CommandLineArgsBuilder argsBuilder)
        {
            BoostTestRunnerSettings adaptedSettings = settings.TestRunnerSettings.Clone();
            adaptedSettings.Timeout = -1;

            // Group by test runner
            IEnumerable<IGrouping<IBoostTestRunner, VSTestCase>> sourceGroups =
                tests.GroupBy((source) => GetTestRunner(source, settings), new BoostTestRunnerComparer()).
                        Where((group) => (group.Key != NullTestRunner.Instance));

            foreach (IGrouping<IBoostTestRunner, VSTestCase> sourceGroup in sourceGroups)
            {
                // Group by test suite
                IEnumerable<IGrouping<string, VSTestCase>> suiteGroups = sourceGroup.GroupBy(test => test.Traits.First(trait => (trait.Name == VSTestModel.TestSuiteTrait)).Value);
                foreach (IGrouping<string, VSTestCase> suiteGroup in suiteGroups)
                {
                    BoostTestRunnerCommandLineArgs args = argsBuilder(sourceGroup.Key.Source, settings);

                    foreach (VSTestCase test in suiteGroup)
                    {
                        // List all tests by display name
                        // but ensure that the first test is fully qualified so that remaining tests are taken relative to this test suite
                        args.Tests.Add((args.Tests.Count == 0) ? test.FullyQualifiedName : test.DisplayName);
                    }

                    yield return new TestRun(sourceGroup.Key, suiteGroup, args, adaptedSettings);
                }
            }
        }

        /// <summary>
        /// Produces test runs, one per test case provided
        /// </summary>
        /// <param name="tests">The tests to prepare in batches</param>
        /// <param name="settings">Adapter settings which are currently in use</param>
        /// <param name="argsBuilder">A builder which produces an appropriate BoostTestRunnerCommandLineArgs structure for a given test and settings pair</param>
        /// <returns>An enumeration of batched test runs, one per test</returns>
        private IEnumerable<TestRun> BatchTestsIndividually(IEnumerable<VSTestCase> tests, BoostTestAdapterSettings settings, CommandLineArgsBuilder argsBuilder)
        {
            return tests.Select(test =>
            {
                IBoostTestRunner runner = GetTestRunner(test, settings);

                if (runner == NullTestRunner.Instance)
                {
                    return null;
                }

                BoostTestRunnerCommandLineArgs args = argsBuilder(runner.Source, settings);
                args.Tests.Add(test.FullyQualifiedName);

                return new TestRun(runner, new VSTestCase[] { test }, args, settings.TestRunnerSettings);
            }).Where((testRun) => (testRun != null));
        }

        /// <summary>
        /// An equality comparer useful for grouping equivalent BoostTestRunners
        /// </summary>
        private class BoostTestRunnerComparer : IEqualityComparer<IBoostTestRunner>
        {
            #region IEqualityComparer<IBoostTestRunner>

            public bool Equals(IBoostTestRunner x, IBoostTestRunner y)
            {
                Utility.Code.Require(x, "x");
                Utility.Code.Require(y, "y");

                return x.Source == y.Source;
            }

            public int GetHashCode(IBoostTestRunner obj)
            {
                Utility.Code.Require(obj, "obj");

                return obj.Source.GetHashCode();
            }

            #endregion IEqualityComparer<IBoostTestRunner>
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

                    CleanOutput(batch.Arguments);

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
                catch (BoostTestAdapter.Boost.Runner.TimeoutException ex)
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
                    Logger.Error("Exception caught while running test batch {0} [{1}] ({2})", batch.Source, string.Join(", ", batch.Tests), ex.Message);
                }
            }
        }

        /// <summary>
        /// Delete output files.
        /// </summary>
        /// <param name="args">The BoostTestRunnerCommandLineArgs which contains references to output files.</param>
        private static void CleanOutput(BoostTestRunnerCommandLineArgs args)
        {
            DeleteFile(args.LogFile);
            DeleteFile(args.ReportFile);
            DeleteFile(args.StandardOutFile);
            DeleteFile(args.StandardErrorFile);
        }

        /// <summary>
        /// Checks to see if the file is available and deletes it.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        /// <returns>true if the file is deleted; false otherwise.</returns>
        private static bool DeleteFile(string file)
        {
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                File.Delete(file);
                return true;
            }

            return false;
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
        /// Factory function which returns an appropriate BoostTestRunnerCommandLineArgs structure
        /// </summary>
        /// <param name="source">The TestCases source</param>
        /// <param name="settings">The Boost Test adapter settings currently in use</param>
        /// <returns>A BoostTestRunnerCommandLineArgs structure for the provided source</returns>
        private BoostTestRunnerCommandLineArgs GetDefaultArguments(string source, BoostTestAdapterSettings settings)
        {
            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs();

            args.WorkingDirectory = Path.GetDirectoryName(source);

            string filename = Path.GetFileName(source);

            // Specify log and report file information
            args.LogFormat = OutputFormat.XML;
            args.LogLevel = settings.LogLevel;
            args.LogFile = SanitizeFileName(filename + FileExtensions.LogFile);

            args.ReportFormat = OutputFormat.XML;
            args.ReportLevel = ReportLevel.Detailed;
            args.ReportFile = SanitizeFileName(filename + FileExtensions.ReportFile);

            args.StandardOutFile = SanitizeFileName(filename + FileExtensions.StdOutFile);
            args.StandardErrorFile = SanitizeFileName(filename + FileExtensions.StdErrFile);

            return args;
        }

        /// <summary>
        /// Factory function which returns an appropriate BoostTestRunnerCommandLineArgs structure for code coverage
        /// </summary>
        /// <param name="source">The TestCases source</param>
        /// <param name="settings">The Boost Test adapter settings currently in use</param>
        /// <returns>A BoostTestRunnerCommandLineArgs structure for the provided source</returns>
        private BoostTestRunnerCommandLineArgs GetCodeCoverageArguments(string source, BoostTestAdapterSettings settings)
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
        /// Sanitizes a file name suitable for Boost Test command line argument values
        /// </summary>
        /// <param name="file">The filename to sanitize.</param>
        /// <returns>The sanitized filename.</returns>
        private static string SanitizeFileName(string file)
        {
            return file.Replace(' ', '_');
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
                string text = File.ReadAllText(testRun.Arguments.ReportFile);

                if (text.Trim() == TestNotFound)
                {
                    return testRun.Tests.Select(GenerateNotFoundResult);
                }
                else
                {
                    // Re-throw the exception
                    throw;
                }
            }

            return testRun.Tests.
                Select(test =>
                {
                    // Locate the test result associated to the current test
                    BoostTestAdapter.Boost.Results.TestResult result = results[test.FullyQualifiedName];

                    if (result != null)
                    {
                        // Convert the Boost.Test.Result data structure into an equivalent Visual Studio model
                        VSTestResult vsResult = result.AsVSTestResult(test);
                        vsResult.StartTime = start;
                        vsResult.EndTime = end;

                        return vsResult;
                    }

                    return null;
                }).
                Where(result => (result != null));
        }

        /// <summary>
        /// Generates a default TestResult for a timeout exception.
        /// </summary>
        /// <param name="test">The test which failed due to a timeout.</param>
        /// <param name="ex">The exception related to this timeout.</param>
        /// <returns>A timed-out, failed TestResult related to the provided test.</returns>
        private static VSTestResult GenerateTimeoutResult(VSTestCase test, BoostTestAdapter.Boost.Runner.TimeoutException ex)
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

        #region Helper classes

        private class NullTestRunner : IBoostTestRunner
        {
            #region IBoostTestRunner

            public void Debug(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings, IFrameworkHandle framework)
            {
                // NO OP
            }

            public void Run(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
            {
                // NO OP
            }

            public string Source
            {
                get { return string.Empty; }
            }

            #endregion IBoostTestRunner

            public static readonly IBoostTestRunner Instance = new NullTestRunner();
        }

        #endregion Helper classes
    }
}