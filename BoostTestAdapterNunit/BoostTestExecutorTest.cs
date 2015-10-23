// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;
using FakeItEasy;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NUnit.Framework;
using TimeoutException = BoostTestAdapter.Boost.Runner.TimeoutException;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using VSTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestExecutorTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.TempDir = null;

            this.RunnerFactory = new StubBoostTestRunnerFactory(this);

            this.Executor = new BoostTestExecutor(
                this.RunnerFactory,
                new StubBoostTestDiscovererFactory(this)
            );

            this.FrameworkHandle = new StubFrameworkHandle();

            this.RunContext = new DefaultTestContext();
        }

        #endregion Test Setup/Teardown

        #region Test Data

        /// <summary>
        /// Test case fully qualified name which should generate a timeout exception.
        /// </summary>
        private const string TimeoutTestCase = "Timeout";

        /// <summary>
        /// Timeout threshold.
        /// </summary>
        private const int Timeout = 10;

        /// <summary>
        /// Default test case fully qualified name.
        /// </summary>
        private string DefaultTestCase
        {
            get
            {
                return "XmlDomInterfaceTestSuite/ParseXmlFileWithoutValidationTest";
            }
        }

        /// <summary>
        /// Fully qualified path to the default test source.
        /// </summary>
        private string DefaultSource
        {
            get
            {
                // Use temporary file path in order to allow NUnit
                // tests to execute within different environments...
                //
                // And to be able to access test result output
                // since test results are placed relative to the
                // test source file.
                return Path.Combine(TempDir, "default");
            }
        }

        private string _tempdir = null;

        private string TempDir
        {
            get
            {
                if (_tempdir == null)
                {
                    _tempdir = Path.GetDirectoryName(Path.GetTempPath());
                }

                return _tempdir;
            }

            set
            {
                this._tempdir = value;
            }
        }

        /// <summary>
        /// Empty test source fully qualified path.
        /// </summary>
        private string EmptySource
        {
            get
            {
                return "empty";
            }
        }

        private StubBoostTestRunnerFactory RunnerFactory { get; set; }
        private BoostTestExecutor Executor { get; set; }
        private StubFrameworkHandle FrameworkHandle { get; set; }
        private DefaultTestContext RunContext { get; set; }

        #endregion Test Data

        #region Stubs/Mocks

        /// <summary>
        /// Utility base class allowing access to the parent class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private abstract class InnerClass<T>
        {
            protected InnerClass(T parent)
            {
                this.Parent = parent;
            }

            /// <summary>
            /// Parent class hosting this inner class.
            /// </summary>
            protected T Parent { get; private set; }
        }

        /// <summary>
        /// Stub ITestDiscovererFactory implementation. Generates StubBoostTestDiscoverer instances.
        /// </summary>
        private class StubBoostTestDiscovererFactory : InnerClass<BoostTestExecutorTest>, IBoostTestDiscovererFactory
        {
            public StubBoostTestDiscovererFactory(BoostTestExecutorTest parent) :
                base(parent)
            {
            }

            #region ITestDiscovererFactory

            public IBoostTestDiscoverer GetDiscoverer(string source, BoostTestAdapterSettings options)
            {
                return new StubBoostTestDiscoverer(this.Parent);
            }

            public IEnumerable<FactoryResult> GetDiscoverers(IReadOnlyCollection<string> sources, BoostTestAdapterSettings settings)
            {
                return new List<FactoryResult>()
                {
                    new FactoryResult()
                    {
                        Discoverer = new StubBoostTestDiscoverer(this.Parent), 
                        Sources = sources
                    }
                };
            }

            #endregion ITestDiscovererFactory
        }

        /// <summary>
        /// Stub ITestDiscoverer implementation. Based on the requested source, generates fake discovery results.
        /// </summary>
        private class StubBoostTestDiscoverer : InnerClass<BoostTestExecutorTest>, IBoostTestDiscoverer
        {
            public StubBoostTestDiscoverer(BoostTestExecutorTest parent) :
                base(parent)
            {
            }

            #region ITestDiscoverer

            public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
            {
                foreach (string source in sources)
                {
                    foreach (VSTestCase test in this.Parent.GetTests(source))
                    {
                        discoverySink.SendTestCase(test);
                    }
                }
            }

            #endregion ITestDiscoverer
        }

        /// <summary>
        /// Stub IBoostTestRunnerFactory implementation. Provisions fake IBoostTestRunner instances
        /// which simulate certain conditions based on the requested source.
        /// </summary>
        private class StubBoostTestRunnerFactory : InnerClass<BoostTestExecutorTest>, IBoostTestRunnerFactory
        {
            public StubBoostTestRunnerFactory(BoostTestExecutorTest parent) :
                base(parent)
            {
                this.ProvisionedRunners = new List<IBoostTestRunner>();
            }

            public IList<IBoostTestRunner> ProvisionedRunners { get; private set; }

            /// <summary>
            /// Reference to the latest IBoostTestRunner which was provisioned by this factory.
            /// </summary>
            public IBoostTestRunner LastTestRunner
            {
                get { return this.ProvisionedRunners.LastOrDefault(); }
            }

            #region IBoostTestRunnerFactory

            public IBoostTestRunner GetRunner(string identifier, BoostTestRunnerFactoryOptions options)
            {
                switch (identifier)
                {
                    case TimeoutTestCase:
                        {
                            IBoostTestRunner timeoutRunner = A.Fake<IBoostTestRunner>();
                            A.CallTo(() => timeoutRunner.Source).Returns(identifier);
                            A.CallTo(() => timeoutRunner.Run(A<BoostTestRunnerCommandLineArgs>._, A<BoostTestRunnerSettings>._)).Throws(new TimeoutException(Timeout));
                            A.CallTo(() => timeoutRunner.Debug(A<BoostTestRunnerCommandLineArgs>._, A<BoostTestRunnerSettings>._, A<IFrameworkHandle>._)).Throws(new TimeoutException(Timeout));

                            return Provision(timeoutRunner);
                        }
                }

                return Provision(new MockBoostTestRunner(this.Parent, identifier));
            }

            #endregion IBoostTestRunnerFactory

            private IBoostTestRunner Provision(IBoostTestRunner runner)
            {
                this.ProvisionedRunners.Add(runner);
                return runner;
            }
        }

        /// <summary>
        /// Mock IBoostTestRunner implementation.
        /// 
        /// - Provides access to the latest call information for post-request checking.
        /// - Allows for mocking test results by using temporary files which can be accessed by the rest of the system.
        /// </summary>
        private class MockBoostTestRunner : InnerClass<BoostTestExecutorTest>, IBoostTestRunner
        {
            #region Constructors

            public MockBoostTestRunner(BoostTestExecutorTest parent, string source) :
                base(parent)
            {
                this.Source = source;
                this.DebugExecution = false;
                this.RunCount = 0;
            }

            #endregion Constructors

            #region Properties

            public bool DebugExecution { get; private set; }
            public BoostTestRunnerCommandLineArgs Args { get; private set; }
            public BoostTestRunnerSettings Settings { get; private set; }
            public uint RunCount { get; private set; }

            #endregion Properties

            #region IBoostTestRunner

            public void Debug(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings, IFrameworkHandle framework)
            {
                this.DebugExecution = true;

                Execute(args, settings);
            }

            public void Run(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
            {
                Execute(args, settings);
            }

            public string Source { get; private set; }

            #endregion IBoostTestRunner

            private void Execute(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
            {
                ++this.RunCount;

                this.Args = args;
                this.Settings = settings;

                Assert.That(args.ReportFile, Is.Not.Null);
                Assert.That(args.ReportFormat, Is.EqualTo(OutputFormat.XML));

                Assert.That(args.LogFile, Is.Not.Null);
                Assert.That(args.LogFormat, Is.EqualTo(OutputFormat.XML));

                Assert.That(Path.GetDirectoryName(args.ReportFile), Is.EqualTo(this.Parent.TempDir));
                Assert.That(Path.GetDirectoryName(args.LogFile), Is.EqualTo(this.Parent.TempDir));

                if (!string.IsNullOrEmpty(args.StandardOutFile))
                {
                    Assert.That(Path.GetDirectoryName(args.StandardOutFile), Is.EqualTo(this.Parent.TempDir));
                }

                // Copy the default result files to a temporary location so that they can eventually be read as a TestResultCollection

                foreach (string test in args.Tests)
                {
                    if (ShouldSkipTest(test))
                    {
                        Copy("BoostTestAdapterNunit.Resources.ReportsLogs.NoMatchingTests.sample.test.report.xml", args.ReportFile);
                        Copy("BoostTestAdapterNunit.Resources.ReportsLogs.NoMatchingTests.sample.test.log.xml", args.LogFile);
                    }
                    else
                    {
                        Copy("BoostTestAdapterNunit.Resources.ReportsLogs.PassedTest.sample.test.report.xml", args.ReportFile);
                        Copy("BoostTestAdapterNunit.Resources.ReportsLogs.PassedTest.sample.test.log.xml", args.LogFile);
                    }
                }
            }

            private bool ShouldSkipTest(string test)
            {
                return test.Contains(' ') || test.Contains(',');
            }

            private void Copy(string embeddedResource, string path)
            {
                using (Stream inStream = TestHelper.LoadEmbeddedResource(embeddedResource))
                using (FileStream outStream = File.Create(path))
                {
                    inStream.CopyTo(outStream);
                }
            }
        }

        /// <summary>
        /// Stub IFrameworkHandle implementation. Allows access to recorded TestResults.
        /// </summary>
        private class StubFrameworkHandle : ConsoleMessageLogger, IFrameworkHandle
        {
            public StubFrameworkHandle()
            {
                this.Results = new List<VSTestResult>();
            }

            public ICollection<VSTestResult> Results { get; private set; }

            #region IFrameworkHandle

            public bool EnableShutdownAfterTestRun
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public int LaunchProcessWithDebuggerAttached(string filePath, string workingDirectory, string arguments, IDictionary<string, string> environmentVariables)
            {
                throw new NotImplementedException();
            }

            #endregion IFrameworkHandle

            #region ITestExecutionRecorder

            public void RecordAttachments(IList<AttachmentSet> attachmentSets)
            {
                throw new NotImplementedException();
            }

            public void RecordEnd(VSTestCase testCase, TestOutcome outcome)
            {
                throw new NotImplementedException();
            }

            public void RecordResult(VSTestResult testResult)
            {
                this.Results.Add(testResult);
            }

            public void RecordStart(VSTestCase testCase)
            {
                throw new NotImplementedException();
            }

            #endregion ITestExecutionRecorder
        }

        #endregion Stubs/Mocks

        #region Helper Methods

        /// <summary>
        /// Factory function which returns an enumeration of tests based on the provided test source
        /// </summary>
        /// <param name="source">The test source</param>
        /// <returns>An enumeration of tests related to the requested source</returns>
        private IEnumerable<VSTestCase> GetTests(string source)
        {
            if (source == DefaultSource)
            {
                return GetDefaultTests();
            }

            return Enumerable.Empty<VSTestCase>();
        }

        /// <summary>
        /// Enumerates a sample collection of tests.
        /// </summary>
        /// <returns>An enumeration of sample test cases</returns>
        private IEnumerable<VSTestCase> GetDefaultTests()
        {
            VSTestCase test = CreateTestCase(
                DefaultTestCase,
                DefaultSource
            );

            return new VSTestCase[] { test };
        }

        /// <summary>
        /// Creates a Visual Studio TestCase based on the provided information
        /// </summary>
        /// <param name="fullyQualifiedName">The fully qualified name of the test case</param>
        /// <param name="source">The test case source</param>
        /// <returns>A Visual Studio TestCase intended for BoostTestExecutor execution</returns>
        private VSTestCase CreateTestCase(string fullyQualifiedName, string source)
        {
            VSTestCase test = new VSTestCase(fullyQualifiedName, BoostTestExecutor.ExecutorUri, source);

            test.Traits.Add(VSTestModel.TestSuiteTrait, QualifiedNameBuilder.FromString(fullyQualifiedName).Pop().ToString());

            return test;
        }

        /// <summary>
        /// Asserts test properties for the DefaultTestCase
        /// </summary>
        /// <param name="results"></param>
        private void AssertDefaultTestResultProperties(ICollection<VSTestResult> results)
        {
            Assert.That(this.FrameworkHandle.Results.Count(), Is.EqualTo(1));

            VSTestResult result = results.First();

            Assert.That(result.ComputerName, Is.EqualTo(Environment.MachineName));

            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Passed));

            Assert.That(result.TestCase.Source, Is.EqualTo(DefaultSource));
            Assert.That(result.TestCase.FullyQualifiedName, Is.EqualTo(DefaultTestCase));
        }

        #endregion Helper Methods

        #region Tests

        /// <summary>
        /// Test execution via the 'Run All' command.
        /// 
        /// Test aims:
        ///     - Ensure that all tests within a source are executed and reported.
        /// </summary>
        [Test]
        public void RunTestsFromSource()
        {
            this.Executor.RunTests(
                new string[] { DefaultSource },
                this.RunContext,
                this.FrameworkHandle
            );

            AssertDefaultTestResultProperties(this.FrameworkHandle.Results);
        }

        /// <summary>
        /// A 'Run All' command on an empty source does not fail.
        /// 
        /// Test aims:
        ///     - Ensure that a request for running no tests operates correctly.
        /// </summary>
        [Test]
        public void RunTestsFromEmptySource()
        {
            this.Executor.RunTests(
                new string[] { EmptySource },
                this.RunContext,
                this.FrameworkHandle
            );

            Assert.That(this.FrameworkHandle.Results.Count(), Is.EqualTo(0));
        }

        /// <summary>
        /// A selection of tests can be executed.
        /// 
        /// Test aims:
        ///     - Ensure that when users select a test selection, only those tests are executed.
        /// </summary>
        [Test]
        public void RunTestSelection()
        {
            this.Executor.RunTests(
                GetDefaultTests(),
                this.RunContext,
                this.FrameworkHandle
            );

            AssertDefaultTestResultProperties(this.FrameworkHandle.Results);
        }

        /// <summary>
        /// Debug test runs are available when selecting 'Debug Tests' for a test selection from the test adapter.
        /// 
        /// Test aims:
        ///     - Ensure that when users select to perform a debug test run, a debug run is actually performed.
        /// </summary>
        [Test]
        public void DebugTestSelection()
        {
            this.RunContext.IsBeingDebugged = true;

            this.Executor.RunTests(
                GetDefaultTests(),
                this.RunContext,
                this.FrameworkHandle
            );

            MockBoostTestRunner runner = this.RunnerFactory.LastTestRunner as MockBoostTestRunner;

            Assert.That(runner, Is.Not.Null);
            Assert.That(runner.DebugExecution, Is.True);

            AssertDefaultTestResultProperties(this.FrameworkHandle.Results);
        }

        /// <summary>
        /// Given a valid .runsettings, test execution should respect the configuration.
        /// 
        /// Test aims:
        ///     - Ensure that test execution is able to interpret valid .runsettings.
        /// </summary>
        [Test]
        public void RunTestsWithTestSettings()
        {
            this.RunContext.RegisterSettingProvider(BoostTestAdapterSettings.XmlRootName, new BoostTestAdapterSettingsProvider());
            this.RunContext.LoadEmbeddedSettings("BoostTestAdapterNunit.Resources.Settings.sample.runsettings");

            this.Executor.RunTests(
                GetDefaultTests(),
                this.RunContext,
                this.FrameworkHandle
            );

            AssertDefaultTestResultProperties(this.FrameworkHandle.Results);

            MockBoostTestRunner runner = this.RunnerFactory.LastTestRunner as MockBoostTestRunner;

            Assert.That(runner, Is.Not.Null);

            Assert.That(runner.Settings.Timeout, Is.EqualTo(600000));
        }

        /// <summary>
        /// Given a test fully-qualified names which contain characters which are not compatible with the Boost Test
        /// command-line, generate a 'test not found' notification to the user.
        /// 
        /// Test aims:
        ///     - Ensure that tests which cannot be individually referenced from Boost Test command line are identified
        ///       and marked as skipped.
        /// </summary>
        [Test]
        public void TestSkipEdgeCases()
        {
            this.Executor.RunTests(
                new VSTestCase[] {
                    CreateTestCase("my_test<unsigned char>", DefaultSource),
                    CreateTestCase("boost::bind(my_other_test,3)", DefaultSource)
                },
                this.RunContext,
                this.FrameworkHandle
            );

            Assert.That(this.FrameworkHandle.Results.Count(), Is.EqualTo(2));

            foreach (VSTestResult result in this.FrameworkHandle.Results)
            {
                Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Skipped));
            }
        }

        /// <summary>
        /// Given a long running test, test execution should stop after a pre-determined timeout threshold and inform the user accordingly.
        /// 
        /// Test aims:
        ///     - Ensure that with proper configuration, long running tests generate a timeout test failure.
        /// </summary>
        [Test]
        public void TestTimeoutException()
        {
            this.Executor.RunTests(
                new VSTestCase[] { CreateTestCase("test", TimeoutTestCase) },
                this.RunContext,
                this.FrameworkHandle
            );

            Assert.That(this.FrameworkHandle.Results.Count(), Is.EqualTo(1));

            VSTestResult result = this.FrameworkHandle.Results.First();

            Assert.That(result.Outcome, Is.EqualTo(TestOutcome.Failed));
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromMilliseconds(Timeout)));
            Assert.That(result.ErrorMessage.ToLowerInvariant().Contains("timeout"), Is.True);
        }

        /// <summary>
        /// Given a request for code coverage on all tests, tests should execute as usual, possibly in an optimized manner.
        /// 
        /// Test aims:
        ///     - Ensure that it is possible to run code coverage on a tests of a particular source.
        /// </summary>
        [Test]
        public void TestCodeCoverage()
        {
            this.RunContext.IsDataCollectionEnabled = true;

            this.Executor.RunTests(
                new string[] { DefaultSource },
                this.RunContext,
                this.FrameworkHandle
            );

            IList<MockBoostTestRunner> runners = this.RunnerFactory.ProvisionedRunners.OfType<MockBoostTestRunner>().ToList();

            // Although multiple runners (one per testcase) will be provisioned, only one type of runner (specific to DefaultSource) is used
            Assert.That(runners.GroupBy(runner => runner.Source).Count(), Is.EqualTo(1));

            // Only one runner is executed and that runner is only executed once 
            MockBoostTestRunner testRunner = runners.FirstOrDefault(runner => runner.RunCount == 1);

            Assert.That(testRunner, Is.Not.Null);

            // All tests are executed
            Assert.That(testRunner.Args.Tests, Is.Empty);
        }

        /// <summary>
        /// Given a request for code coverage on selected tests, tests should execute as usual, possibly in an optimized manner.
        /// 
        /// Test aims:
        ///     - Ensure that it is possible to run code coverage on a selection of tests.
        /// </summary>
        [Test]
        public void TestCodeCoverageSelection()
        {
            this.RunContext.IsDataCollectionEnabled = true;

            this.Executor.RunTests(
                new VSTestCase[] { CreateTestCase("Test1", DefaultSource), CreateTestCase("Test2", DefaultSource) },
                this.RunContext,
                this.FrameworkHandle
            );

            IList<MockBoostTestRunner> runners = this.RunnerFactory.ProvisionedRunners.OfType<MockBoostTestRunner>().ToList();

            Assert.That(runners.GroupBy(runner => runner.Source).Count(), Is.EqualTo(1));

            MockBoostTestRunner testRunner = runners.FirstOrDefault(runner => runner.RunCount == 1);

            Assert.That(testRunner, Is.Not.Null);

            // All selected tests are executed
            Assert.That(testRunner.Args.Tests.Count(), Is.EqualTo(2));
        }

        #endregion Tests
    }
}