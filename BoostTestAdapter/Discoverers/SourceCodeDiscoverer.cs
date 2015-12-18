// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BoostTestAdapter.Settings;
using BoostTestAdapter.SourceFilter;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using VisualStudioAdapter;

namespace BoostTestAdapter.Discoverers
{
    /// <summary>
    /// A Boost Test Discoverer that parses the source code of the source project to discover tests.
    /// </summary>
    internal class SourceCodeDiscoverer : IBoostTestDiscoverer
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

        #region Members

        /// <summary>
        /// The Visual Studio instance provider
        /// </summary>
        private readonly IVisualStudioInstanceProvider _vsProvider;

        /// <summary>
        /// Collection of source filters which are applied to sources for correct test extraction
        /// </summary>
        private ISourceFilter[] _sourceFilters;

        #endregion Members

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public SourceCodeDiscoverer()
            : this(new DefaultVisualStudioInstanceProvider())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">The Visual Studio instance provider</param>
        public SourceCodeDiscoverer(IVisualStudioInstanceProvider provider)
        {
            _vsProvider = provider;
        }

        #endregion

        #region IBoostTestDiscoverer

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            Code.Require(sources, "sources");
            Code.Require(discoverySink, "discoverySink");

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(discoveryContext);
            _sourceFilters = SourceFilterFactory.Get(settings);
            IDictionary<string, ProjectInfo> solutioninfo = null;

            var numberOfAttempts = 100;

            // try several times to overcome "Application is Busy" COMException
            while (numberOfAttempts > 0)
            {
                try
                {
                    solutioninfo = PrepareTestCaseData(sources);
                    // set numberOfAttempts = 0, because there is no need to try again,
                    // since obviously no exception was thrown at this point
                    numberOfAttempts = 0;
                }
                catch (COMException)
                {
                    --numberOfAttempts;

                    // re-throw after all attempts have failed
                    if (numberOfAttempts == 0)
                    {
                        throw;
                    }
                }
            }

            GetBoostTests(solutioninfo, discoverySink);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// gets (parses) all testcases from cpp files checking for
        /// BOOST_AUTO_TEST_CASE and BOOST_AUTO_TEST_SUITE parameter
        /// </summary>
        /// <param name="solutionInfo">mapping between projectexe and the corresponding .cpp files</param>
        /// <param name="discoverySink">UTF component for collecting testcases</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void GetBoostTests(IDictionary<string, ProjectInfo> solutionInfo, ITestCaseDiscoverySink discoverySink)
        {
            if (solutionInfo != null)
            {
                foreach (KeyValuePair<string, ProjectInfo> info in solutionInfo)
                {
                    string source = info.Key;
                    ProjectInfo projectInfo = info.Value;

                    foreach (var sourceFile in projectInfo.CppSourceFiles)
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

                                    // Filter out any false positives and quickly reject files which do not contain any Boost Unit Test eye-catchers
                                    if ( ShouldConsiderSourceFile(cppSourceFile.SourceCode) )
                                    {
                                        /*
                                         * it is important that the pre-processor defines at project level are not modified
                                         * because every source file in the project has to have the same starting point.
                                         */

                                        //call to cpy ctor
                                        Defines definitions = new Defines(projectInfo.DefinesHandler);

                                        ApplySourceFilter(cppSourceFile, definitions);
                                        DiscoverBoostTests(cppSourceFile, source, discoverySink);
                                    }
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

        private static bool ShouldConsiderSourceFile(string source)
        {
            // NOTE Using .Contains can be slightly faster then performing a Regex.IsMatch for a set of strings
            // Reference: http://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
            return source.Contains("BOOST_");
        }

        private static int ScrollLines(int lineNumber, ref string line, string[] code)
        {
            int openBracketCount;
            int closeBracketCount;
            string currentLine;
            for 
            (
                openBracketCount = closeBracketCount = 0, currentLine = line; 
                lineNumber <= code.Length;
                currentLine = code[lineNumber++], line += currentLine
            )
            {
                openBracketCount += currentLine.Count(c => c == '(');
                closeBracketCount += currentLine.Count(c => c == ')');
                if (openBracketCount < closeBracketCount)
                    throw new FormatException("Wrong test format");

                if (openBracketCount == closeBracketCount && openBracketCount > 0) return lineNumber;
            }

            throw new FormatException("Unexpected end of file");
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

            for (sourceInfo.LineNumber = 1; sourceInfo.LineNumber <= code.Length; ++sourceInfo.LineNumber)
            {
                string line = code[sourceInfo.LineNumber - 1];

                string[] splitMacro = line.Split('<', '>', '(', ',', ')', ';');
                string desiredMacro = splitMacro[0].Trim();

                // serge: BOOST multiline test macros are supported now
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
                            int newLineNumber = ScrollLines(sourceInfo.LineNumber, ref line, code);
                            if (sourceInfo.LineNumber != newLineNumber)
                            {
                                // recalc splitMacro
                                splitMacro = line.Split('<', '>', '(', ',', ')', ';');
                                sourceInfo.LineNumber = newLineNumber;
                            }

                            string listName = splitMacro[3].Trim();
                            //third parameter is the corresponding boost::mpl::list name

                            if (templateLists.ContainsKey(listName))
                            {
                                foreach (var dataType in templateLists[listName])
                                {
                                    string testCaseName = splitMacro[1].Trim();
                                    //first parameter is the test case name
                                    string testCaseNameWithDataType = testCaseName + "<" + dataType + ">";

                                    var testCase = TestCaseUtils.CreateTestCase(source, sourceInfo, suite, testCaseNameWithDataType);

                                    TestCaseUtils.AddTestCase(testCase, discoverySink);
                                }
                            }
                            break;
                        }

                    case Constants.FixtureTestSuiteIdentifier:
                    case Constants.AutoTestSuiteIdentifier:
                        {
                            int newLineNumber = ScrollLines(sourceInfo.LineNumber, ref line, code);
                            if (sourceInfo.LineNumber != newLineNumber)
                            {
                                // recalc splitMacro
                                splitMacro = line.Split('<', '>', '(', ',', ')', ';');
                                sourceInfo.LineNumber = newLineNumber;
                            }

                            suite.Push(splitMacro[1].Trim());
                            break;
                        }

                    case Constants.FixtureTestCaseIdentifier:
                    case Constants.AutoTestCaseIdentifier:
                        {
                            int newLineNumber = ScrollLines(sourceInfo.LineNumber, ref line, code);
                            if (sourceInfo.LineNumber != newLineNumber)
                            {
                                // recalc splitMacro
                                splitMacro = line.Split('<', '>', '(', ',', ')', ';');
                                sourceInfo.LineNumber = newLineNumber;
                            }

                            string testCaseName = splitMacro[1].Trim();

                            var testCase = TestCaseUtils.CreateTestCase(source, sourceInfo, suite, testCaseName);

                            TestCaseUtils.AddTestCase(testCase, discoverySink);
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
        private IDictionary<string, ProjectInfo> PrepareTestCaseData(IEnumerable<string> sources)
        {
            Dictionary<string, ProjectInfo> solutionInfo = new Dictionary<string, ProjectInfo>();

            // Get the currently loaded VisualStudio instance
            var vs = _vsProvider.Instance;

            if (vs != null)
            {
                // Copy the enumerable to a list so that we can maintain/modify this local copy
                List<string> sourcesCopy = sources.ToList();

                foreach (var project in vs.Solution.Projects)
                {
                    var configuration = project.ActiveConfiguration;

                    //Iterating over projects and then the sources for improved performance
                    int index = sourcesCopy.FindIndex(source => string.Equals(source, configuration.PrimaryOutput, StringComparison.Ordinal));
                    if (index != -1)
                    {
                        var projectInfo = new ProjectInfo(sourcesCopy[index]);

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

        #endregion Private methods
    }
}
