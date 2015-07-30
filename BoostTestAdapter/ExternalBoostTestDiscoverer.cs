using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using TestCase = BoostTestAdapter.Boost.Test.TestCase;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter
{
    /// <summary>
    /// A Boost Test Discoverer which discovers tests based on configuration.
    /// </summary>
    public class ExternalBoostTestDiscoverer : IBoostTestDiscoverer
    {
        #region Constants

        private const string ListFileSuffix = ".test.list.xml";

        #endregion Constants

        public ExternalBoostTestDiscoverer(ExternalBoostTestRunnerSettings settings)
        {
            this.Settings = settings;
        }

        public ExternalBoostTestRunnerSettings Settings { get; private set; }

        #region IBoostTestDiscoverer

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            Utility.Code.Require(sources, "sources");
            Utility.Code.Require(discoverySink, "discoverySink");

            foreach (string source in sources)
            {
                TestFramework framework = DiscoverTestFramework(source);

                if ((framework != null) && (framework.MasterTestSuite != null))
                {
                    BoostTestCaseDiscoverer frameworkDiscoverer = new BoostTestCaseDiscoverer(source, discoverySink);
                    framework.MasterTestSuite.Apply(frameworkDiscoverer);
                }
            }
        }

        #endregion IBoostTestDiscoverer

        /// <summary>
        /// Based on the establishsed configuration, discovers the tests within the provided test source module.
        /// </summary>
        /// <param name="source">The test source module</param>
        /// <returns>The test framework describing all tests contained within the test source or null if one cannot be provided.</returns>
        private TestFramework DiscoverTestFramework(string source)
        {
            if (this.Settings.DiscoveryMethodType == DiscoveryMethodType.DiscoveryFileMap)
            {
                return ParseStaticTestList(source);
            }
            else if (this.Settings.DiscoveryMethodType == DiscoveryMethodType.DiscoveryCommandLine)
            {
                return ExecuteExternalDiscoveryCommand(source);
            }

            return null;
        }

        /// <summary>
        /// Executes the discovery command as specified in the configuration for the requested test source.
        /// </summary>
        /// <param name="source">The test source module</param>
        /// <returns>The test framework describing all tests contained within the test source or null if one cannot be provided.</returns>
        private TestFramework ExecuteExternalDiscoveryCommand(string source)
        {
            // Use a temporary file to host the result of the external discovery process
            string path = Path.Combine(Path.GetDirectoryName(source), Path.GetFileName(source) + ListFileSuffix);

            // Perform cleanup to avoid inconsistent listing
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            CommandEvaluator evaluator = new CommandEvaluator();

            evaluator.SetVariable("source", source);
            evaluator.SetVariable("out", path);

            // Evaluate the discovery command
            CommandLine commandLine = new CommandLine
            {
                FileName = evaluator.Evaluate(this.Settings.DiscoveryCommandLine.FileName).Result,
                Arguments = evaluator.Evaluate(this.Settings.DiscoveryCommandLine.Arguments).Result
            };

            // Execute the discovery command via an external process
            if (ExecuteCommand(commandLine))
            {
                // Parse the generate TestFramework from the temporary file
                return ParseTestFramework(path);
            }

            return null;
        }

        /// <summary>
        /// Executes the provided command line as an external process.
        /// </summary>
        /// <param name="commandLine">The process command line</param>
        /// <returns>true if the process terminated successfully (exit code = 0); false otherwise.</returns>
        private static bool ExecuteCommand(CommandLine commandLine)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetTempPath(),
                FileName = commandLine.FileName,
                Arguments = commandLine.Arguments,
                RedirectStandardError = false,
                RedirectStandardInput = false
            };
            
            Process process = Process.Start(ProcessStartInfoEx.StartThroughCmdShell(info));
            if (process != null)
            {
                process.WaitForExit();

                return (process.ExitCode == 0);
            }

            return false;
        }

        /// <summary>
        /// Parses a static file containing the test lising for the requested test source as specified in the configuration.
        /// </summary>
        /// <param name="source">The test source module</param>
        /// <returns>The test framework describing all tests contained within the test source or null if one cannot be provided.</returns>
        private TestFramework ParseStaticTestList(string source)
        {
            string path = null;
            if (this.Settings.DiscoveryFileMap.TryGetValue(Path.GetFileName(source), out path))
            {
                return ParseTestFramework(path);   
            }

            return null;
        }

        /// <summary>
        /// Deserializes a TestFramework from the Xml file path provided.
        /// </summary>
        /// <param name="path">A valid path to a TestFramework Xml file.</param>
        /// <returns>The deserialized TestFramework</returns>
        private static TestFramework ParseTestFramework(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(TestFramework));
                return deserializer.Deserialize(stream) as TestFramework;
            }
        }

        /// <summary>
        /// ITestVisitor implementation. Visits TestCases and registers them
        /// with the supplied ITestCaseDiscoverySink.
        /// </summary>
        private class BoostTestCaseDiscoverer : ITestVisitor
        {
            #region Constructors

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="source">The source test module which contains the discovered tests</param>
            /// <param name="sink">The ITestCaseDiscoverySink which will have tests registered with</param>
            public BoostTestCaseDiscoverer(string source, ITestCaseDiscoverySink sink)
            {
                this.Source = source;
                this.Sink = sink;

                this.TestSuite = new QualifiedNameBuilder();
            }

            #endregion Constructors

            #region Properties

            public ITestCaseDiscoverySink Sink { get; private set; }

            private QualifiedNameBuilder TestSuite { get; set; }

            public uint Count { get; private set; }

            private TestSuite MasterTestSuite { get; set; }

            public string Source { get; private set; }

            #endregion Properties

            public void Visit(Boost.Test.TestCase testCase)
            {
                Utility.Code.Require(testCase, "testCase");

                // Convert from Boost.Test.TestCase to a Visual Studio TestCase object
                VSTestCase test = GenerateTestCase(testCase);

                if (test != null)
                {
                    Logger.Info("Found test: {0}", testCase.FullyQualifiedName);

                    ++this.Count;

                    // Register test case
                    this.Sink.SendTestCase(test);
                }
            }

            public void Visit(TestSuite testSuite)
            {
                Utility.Code.Require(testSuite, "testSuite");

                this.TestSuite.Push(testSuite);

                // Identify Master Test Suite
                if ((this.MasterTestSuite == null) && (testSuite.Parent == null))
                {
                    this.MasterTestSuite = testSuite;
                }

                foreach (TestUnit child in testSuite.Children)
                {
                    child.Apply(this);
                }

                this.TestSuite.Pop();
            }

            /// <summary>
            /// Generates a Visual Studio equivalent test case structure.
            /// </summary>
            /// <param name="testCase">The Boost.Test.TestCase to convert.</param>
            /// <returns>An equivalent Visual Studio TestCase structure to the one provided.</returns>
            private VSTestCase GenerateTestCase(TestCase testCase)
            {
                // Temporarily push TestCase on TestSuite name builder to acquire the fully qualified name of the TestCase
                this.TestSuite.Push(testCase);

                VSTestCase test = new VSTestCase(
                    this.TestSuite.ToString(),
                    BoostTestExecutor.ExecutorUri,
                    this.Source
                );

                // Reset TestSuite QualifiedNameBuilder to original value
                this.TestSuite.Pop();

                if (testCase.Source != null)
                {
                    test.CodeFilePath = testCase.Source.File;
                    test.LineNumber = testCase.Source.LineNumber;
                }

                test.DisplayName = testCase.Name;

                // Register the test suite as a trait
                test.Traits.Add(new Trait(VSTestModel.TestSuiteTrait, GetCurrentTestSuite()));

                return test;
            }

            /// <summary>
            /// Provides the fully qualified name of the current TestSuite
            /// </summary>
            /// <returns>The fully qualified name of the current TestSuite</returns>
            private string GetCurrentTestSuite()
            {
                // Since the master test suite name is not included in the fully qualified name, identify
                // this edge case and explicitly return the master test suite name in such cases.
                if (this.TestSuite.Level == QualifiedNameBuilder.MasterTestSuiteLevel)
                {
                    return (this.MasterTestSuite == null) ? QualifiedNameBuilder.DefaultMasterTestSuiteName : this.MasterTestSuite.Name;
                }

                return this.TestSuite.ToString();
            }
        }
    }
}
