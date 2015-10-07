// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter.SourceFilter;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using VisualStudioAdapter;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter
{
    /// <summary>
    /// Contains method to find boost test cases from list of files
    /// </summary>
    public class BoostTestDiscovererInternal
    {
        #region Constants

        /// <summary>
        /// Constants identifying Boost Test tokens.
        /// </summary>
        private static class Constants
        {
            public const string TypedefListIdentifier = "typedef list";
            public const string TypedefMplListIdentifier = "typedef mpl::list";
            public const string TypedefBoostMplListIdentifier = "typedef boost::mpl::list";
            public const string AutoTestCaseIdentifier = "BOOST_AUTO_TEST_CASE";
            public const string FixtureTestCaseIdentifier = "BOOST_FIXTURE_TEST_CASE";
            public const string AutoTestSuiteIdentifier = "BOOST_AUTO_TEST_SUITE";
            public const string FixtureTestSuiteIdentifier = "BOOST_FIXTURE_TEST_SUITE";
            public const string AutoTestSuiteEndIdentifier = "BOOST_AUTO_TEST_SUITE_END";
            public const string TestCaseTemplateIdentifier = "BOOST_AUTO_TEST_CASE_TEMPLATE";
        }

        #endregion Constants

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">The Visual Studio instance provider</param>
        /// <param name="newSourceFilters">source filters object that will be used to filter inactive code</param>
        public BoostTestDiscovererInternal(IVisualStudioInstanceProvider provider, ISourceFilter[] newSourceFilters)
        {
            this.VSProvider = provider;
            this._sourceFilters = newSourceFilters;
        }

        #region Members

        /// <summary>
        /// Collection of source filters which are applied to sources for correct test extraction
        /// </summary>
        private readonly ISourceFilter[] _sourceFilters;

        #endregion Members

        #region Properties

        /// <summary>
        /// Visual Studio Instance provider
        /// </summary>
        public IVisualStudioInstanceProvider VSProvider { get; private set; }

        #endregion Properties

        #region Public methods

        /// <summary>
        /// gets (parses) all testcases from cpp files checking for
        /// BOOST_AUTO_TEST_CASE and BOOST_AUTO_TEST_SUITE parameter
        /// </summary>
        /// <param name="solutionInfo">mapping between projectexe and the corresponding .cpp files</param>
        /// <param name="discoverySink">UTF component for collecting testcases</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void GetBoostTests(IDictionary<string, ProjectInfo> solutionInfo, ITestCaseDiscoverySink discoverySink)
        {
            if (solutionInfo != null)
            {
                foreach (KeyValuePair<string, ProjectInfo> info in solutionInfo)
                {
                    string source = info.Key;
                    ProjectInfo projectInfo = info.Value;

                    foreach(var sourceFile in projectInfo.CppSourceFiles)
                    {
                        try
                        {
                            using (var sr = new StreamReader(sourceFile))
                            {
                                try
                                {
                                    var cppSourceFile = new CppSourceFile()
                                    {
                                        FileName = sourceFile,
                                        SourceCode = sr.ReadToEnd()
                                    };

                                    /*
                                     * it is important that the pre-processor defines at project level are not modified
                                     * because every source file in the project has to have the same starting point.
                                     */

                                    ApplySourceFilter(cppSourceFile, new Defines(projectInfo.DefinesHandler));
                                    //call to cpy ctor
                                    DiscoverBoostTests(cppSourceFile, source, discoverySink);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(
                                            "Exception raised while discovering tests from \"{0}\" of project \"{1}\", ({2})",
                                            sourceFile, projectInfo.ProjectExe, ex.Message);
                                    Logger.Error(ex.StackTrace);
                                }
                            }
                        }
                        catch
                        {
                            Logger.Error("Unable to open file \"{0}\" of project \"{1}\".", sourceFile, projectInfo.ProjectExe);
                        }
                    }
                }
            }
            else
            {
                Logger.Error("the solutionInfo object was found to be null whilst");
            }
        }

        /// <summary>
        /// Discovers Boost Test from the provided C++ source file. Notifies test discovery via the provided discoverySink.
        /// </summary>
        /// <param name="cppSourceFile">The C++ source file to scan for Boost Tests</param>
        /// <param name="source">The associated test source EXE</param>
        /// <param name="discoverySink">The discoverySink to which identified tests will be notified to</param>
        private static void DiscoverBoostTests(CppSourceFile cppSourceFile, string source, ITestCaseDiscoverySink discoverySink)
        {
            string[] code = cppSourceFile.SourceCode.TrimEnd(new[] { ' ', '\n', '\r' }).Split('\n');

            SourceFileInfo sourceInfo = new SourceFileInfo(cppSourceFile.FileName, 0);

            QualifiedNameBuilder suite = new QualifiedNameBuilder();
            // Push the equivalent of the Master Test Suite
            suite.Push(QualifiedNameBuilder.DefaultMasterTestSuiteName);

            var templateLists = new Dictionary<string, List<string>>();

            foreach (string line in code)
            {
                ++sourceInfo.LineNumber;

                string[] splitMacro = line.Split(new[] { '<', '>', '(', ',', ')', ';' });
                string desiredMacro = splitMacro[0].Trim();

                /*
                 * Currently the below is not able to handle BOOST UTF signatures spread over multiple lines.
                 */
                switch (desiredMacro)
                {
                    case Constants.TypedefListIdentifier:
                    case Constants.TypedefMplListIdentifier:
                    case Constants.TypedefBoostMplListIdentifier:
                        {
                            var dataTypes = new List<string>();
                            int i;

                            for (i = 1; i < splitMacro.Length - 2; ++i)
                            {
                                dataTypes.Add(splitMacro[i].Trim());
                            }

                            templateLists.Add(splitMacro[i].Trim(), dataTypes);
                            break;
                        }

                    case Constants.TestCaseTemplateIdentifier:
                        {
                            string listName = splitMacro[3].Trim();
                            //third parameter is the corresponding boost::mpl::list name

                            if (templateLists.ContainsKey(listName))
                            {
                                foreach (var dataType in templateLists[listName])
                                {
                                    string testCaseName = splitMacro[1].Trim();
                                    //first parameter is the test case name
                                    string testCaseNameWithDataType = testCaseName + "<" + dataType + ">";

                                    var testCase = CreateTestCase(source, sourceInfo,
                                        suite, testCaseNameWithDataType);

                                    AddTestCase(testCase, discoverySink);
                                }
                            }
                            break;
                        }

                    case Constants.FixtureTestSuiteIdentifier:
                    case Constants.AutoTestSuiteIdentifier:
                        {
                            suite.Push(splitMacro[1].Trim());
                            break;
                        }

                    case Constants.FixtureTestCaseIdentifier:
                    case Constants.AutoTestCaseIdentifier:
                        {
                            string testCaseName = splitMacro[1].Trim();

                            var testCase = CreateTestCase(source, sourceInfo, suite,
                                testCaseName);

                            AddTestCase(testCase, discoverySink);
                            break;
                        }

                    case Constants.AutoTestSuiteEndIdentifier:
                        {
                            suite.Pop();
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Applies the filter actions created in the _sourceFilters object onto the source code
        /// </summary>
        /// <param name="cppSourceFile">source code related information</param>
        /// <param name="definesHandler">reference to the defines instances handling the pre-processor defines</param>
        private void ApplySourceFilter(CppSourceFile cppSourceFile, Defines definesHandler)
        {
            foreach (var sourceFilter in _sourceFilters)
            {
                sourceFilter.Filter(cppSourceFile, definesHandler);
            }
        }

        /// <summary>
        /// Prepares the test case data. Maps each source to a project within the currently loaded solution.
        /// </summary>
        /// <param name="sources">List of exe files present in the solution.</param>
        /// <returns>Dictionary mapping each source to a ProjectInfo that will be populated with the project information</returns>
        public IDictionary<string, ProjectInfo> PrepareTestCaseData(IEnumerable<string> sources)
        {
            Dictionary<string, ProjectInfo> solutionInfo = new Dictionary<string, ProjectInfo>();

            // Get the currently loaded VisualStudio instance
            IVisualStudio vs = this.VSProvider.Instance;

            if (vs != null)
            {
                // Copy the enumerable to a list so that we can maintain/modify this local copy
                List<string> sourcesCopy = sources.ToList();

                foreach (IProject project in vs.Solution.Projects)
                {
                    IProjectConfiguration configuration = project.ActiveConfiguration;

                    //Iterating over projects and then the sources for improved performance
                    int index = sourcesCopy.FindIndex(source => string.Equals(source, configuration.PrimaryOutput, StringComparison.Ordinal));
                    if (index != -1)
                    {
                        ProjectInfo projectInfo = new ProjectInfo(sourcesCopy[index]);

                        // Maintain (copied) list of sources so that we may exit early if possible
                        sourcesCopy.RemoveAt(index);

                        projectInfo.DefinesHandler = configuration.CppCompilerOptions.PreprocessorDefinitions;
                        foreach (string sourceFile in project.SourceFiles)
                        {
                            projectInfo.CppSourceFiles.Add(sourceFile);
                        }

                        solutionInfo.Add(projectInfo.ProjectExe, projectInfo);

                        if (sourcesCopy.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }

            return solutionInfo;
        }

        #endregion Public methods

        #region Private helper methods

        /// <summary>
        /// Creates a new TestCase object.
        /// </summary>
        /// <param name="sourceExe">Name of the project executable</param>
        /// <param name="sourceInfo">.cpp file path and TestCase line number</param>
        /// <param name="suite">The suite in which testcase is present</param>
        /// <param name="testCaseName">Name of the testcase</param>
        /// <returns>The created TestCase object</returns>
        private static VSTestCase CreateTestCase(string sourceExe, SourceFileInfo sourceInfo, QualifiedNameBuilder suite, string testCaseName)
        {
            suite.Push(testCaseName);

            string qualifiedName = suite.ToString();

            suite.Pop();

            var testCase = new VSTestCase(qualifiedName, BoostTestExecutor.ExecutorUri, sourceExe)
            {
                CodeFilePath = sourceInfo.File,
                LineNumber = sourceInfo.LineNumber,
                DisplayName = testCaseName,
            };

            GroupViaTraits(suite.ToString(), testCase);

            return testCase;
        }

        /// <summary>
        /// Sets the Traits property for the testcase object.
        /// </summary>
        /// <param name="suiteName">Name of the test suite to which the testcase belongs</param>
        /// <param name="testCase">[ref] The testcase object</param>
        private static void GroupViaTraits(string suiteName, VSTestCase testCase)
        {
            string traitName = suiteName;

            if (string.IsNullOrEmpty(suiteName))
            {
                traitName = QualifiedNameBuilder.DefaultMasterTestSuiteName;
            }

            testCase.Traits.Add(VSTestModel.TestSuiteTrait, traitName);
        }

        /// <summary>
        /// Helper methods which adds a test case to an internal list and sends the test to the discovery sink
        /// </summary>
        /// <param name="testCase">the test case to be added</param>
        /// <param name="discoverySink">the discovery sink where the test case is sent to</param>
        private static void AddTestCase(VSTestCase testCase, ITestCaseDiscoverySink discoverySink)
        {
            //send to discovery sink
            if (null != discoverySink)
            {
                Logger.Info("Found test: {0}", testCase.FullyQualifiedName);
                discoverySink.SendTestCase(testCase);
            }
        }

        #endregion Private helper methods
    }
}