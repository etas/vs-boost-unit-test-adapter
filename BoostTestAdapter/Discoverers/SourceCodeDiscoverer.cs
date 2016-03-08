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
                                    Logger.Debug(ex.StackTrace);
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

        /// <summary>
        /// Identifies a function-like expression over a series of lines
        /// </summary>
        /// 
        /// <param name="lineNumber">The initial line number of the source line in question</param>
        /// <param name="code">The source code listing seperated by line</param>
        /// <param name="line">The current line to scan. Line will be appended with all subsequent lines which form the function expression</param>
        /// 
        /// <returns>The new line number at which the function-like expression ends</returns>
        /// 
        /// <exception cref="FormatException">Thrown in case function expression is invalid</exception>
        private static int ScrollLines(int lineNumber, string[] code, ref string line)
        {
            // Retain a record of previous bracket counts to avoid re-iterating on computed substrings
            int openBracketCount = 0;
            int closeBracketCount = 0;

            // Offset from source code line from which to start scanning for brackets
            int lineOffset = 0;

            for (;;)
            {
                // Scan for bracket tokens
                for (; lineOffset < line.Length; ++lineOffset)
                {
                    if (line[lineOffset] == '(')
                    {
                        ++openBracketCount;
                    }
                    else if (line[lineOffset] == ')')
                    {
                        ++closeBracketCount;
                    }
                }

                if (openBracketCount < closeBracketCount)
                    throw new FormatException("Wrong test format");

                if ((openBracketCount > 0) && (openBracketCount == closeBracketCount)) break;

                if ((++lineNumber) > code.Length)
                    throw new FormatException("Unexpected end of file");

                line += code[lineNumber - 1];
            }

            return lineNumber;
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

            var templateLists = new Dictionary<string, IEnumerable<string>>();

            for (sourceInfo.LineNumber = 1; sourceInfo.LineNumber <= code.Length; ++sourceInfo.LineNumber)
            {
                string line = code[sourceInfo.LineNumber - 1];

                string[] splitMacro = SplitMacro(line);
                string desiredMacro = splitMacro[0].Trim();
                
                switch (desiredMacro)
                {
                    case Constants.TypedefListIdentifier:
                    case Constants.TypedefMplListIdentifier:
                    case Constants.TypedefBoostMplListIdentifier:
                        {
                            var templateList = ParseTemplateList(splitMacro);
                            templateLists.Add(templateList.Key, templateList.Value);
                            break;
                        }

                    case Constants.TestCaseTemplateIdentifier:
                        {
                            var templateTest = ParseTemplateTestCase(splitMacro, templateLists, sourceInfo, code, ref line);

                            foreach (string testCaseDataType in templateTest.Value)
                            {
                                string testCaseNameWithDataType = templateTest.Key + '<' + testCaseDataType + '>';
                                var testCase = TestCaseUtils.CreateTestCase(source, sourceInfo, suite, testCaseNameWithDataType);
                                TestCaseUtils.AddTestCase(testCase, discoverySink);
                            }
                            break;
                        }

                    case Constants.FixtureTestSuiteIdentifier:
                    case Constants.AutoTestSuiteIdentifier:
                        {
                            string suiteName = ParseBeginTestSuite(splitMacro, sourceInfo, code, ref line);
                            suite.Push(suiteName);
                            break;
                        }

                    case Constants.FixtureTestCaseIdentifier:
                    case Constants.AutoTestCaseIdentifier:
                        {
                            string testCaseName = ParseTestCase(splitMacro, sourceInfo, code, ref line);
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
        /// Parses the declaration of a templated test case
        /// </summary>
        /// <param name="splitMacro">The current source line split into tokens</param>
        /// <param name="definedTemplates">A collection of templated lists defined prior to this test case declaration/definition</param>
        /// <param name="sourceInfo">Source file and line information/param>
        /// <param name="code">The entire code split by line</param>
        /// <param name="line">The current source code line which is being evaluated</param>
        /// <returns>A tuple consisting of the test case name and the list of defined templated types</returns>
        private static KeyValuePair<string, IEnumerable<string>> ParseTemplateTestCase(string[] splitMacro, Dictionary<string, IEnumerable<string>> definedTemplates, SourceFileInfo sourceInfo, string[] code, ref string line)
        {
            int newLineNumber = ScrollLines(sourceInfo.LineNumber, code, ref line);
            if (sourceInfo.LineNumber != newLineNumber)
            {
                // recalc splitMacro
                splitMacro = SplitMacro(line);
                sourceInfo.LineNumber = newLineNumber;
            }

            //third parameter is the corresponding boost::mpl::list name
            string listName = splitMacro[3].Trim();

            string testCaseName = splitMacro[1].Trim();

            IEnumerable<string> templatedDataTypes = Enumerable.Empty<string>();
            if (definedTemplates.ContainsKey(listName))
            {
                templatedDataTypes = definedTemplates[listName];
            }

            return new KeyValuePair<string, IEnumerable<string>>(testCaseName, templatedDataTypes);
        }

        /// <summary>
        /// Parses the beginning declaration of a fixture or auto test case
        /// </summary>
        /// <param name="splitMacro">The current source line split into tokens</param>
        /// <param name="sourceInfo">Source file and line information/param>
        /// <param name="code">The entire code split by line</param>
        /// <param name="line">The current source code line which is being evaluated</param>
        /// <returns>The name of the test case</returns>
        private static string ParseTestCase(string[] splitMacro, SourceFileInfo sourceInfo, string[] code, ref string line)
        {
            int newLineNumber = ScrollLines(sourceInfo.LineNumber, code, ref line);
            if (sourceInfo.LineNumber != newLineNumber)
            {
                // recalc splitMacro
                splitMacro = SplitMacro(line);
                sourceInfo.LineNumber = newLineNumber;
            }

            return splitMacro[1].Trim();
        }

        /// <summary>
        /// Parses the beginning declaration of a fixture or auto test sute
        /// </summary>
        /// <param name="splitMacro">The current source line split into tokens</param>
        /// <param name="sourceInfo">Source file and line information/param>
        /// <param name="code">The entire code split by line</param>
        /// <param name="line">The current source code line which is being evaluated</param>
        /// <returns>The name of the test sute</returns>
        private static string ParseBeginTestSuite(string[] splitMacro, SourceFileInfo sourceInfo, string[] code, ref string line)
        {
            int newLineNumber = ScrollLines(sourceInfo.LineNumber, code, ref line);
            if (sourceInfo.LineNumber != newLineNumber)
            {
                // recalc splitMacro
                splitMacro = SplitMacro(line);
                sourceInfo.LineNumber = newLineNumber;
            }

            return splitMacro[1].Trim();
        }
        
        /// <summary>
        /// Parses a template type list used for templated Boost Tests
        /// </summary>
        /// <param name="splitMacro">The split source line</param>
        /// <returns>A tuple consisting of the template list identifier and the list of defined templated types</returns>
        private static KeyValuePair<string, IEnumerable<string>> ParseTemplateList(string[] splitMacro)
        {
            var dataTypes = new List<string>();
            int i = 1;

            for (; i < splitMacro.Length - 2; ++i)
            {
                dataTypes.Add(splitMacro[i].Trim());
            }

            return new KeyValuePair<string, IEnumerable<string>>(splitMacro[i].Trim(), dataTypes);
        }

        /// <summary>
        /// Splits a BOOST macro definition into components of interest
        /// </summary>
        /// <param name="line">The source code line which contains the BOOST macro definition</param>
        /// <returns>An array of string components which are of interest for further evaluation</returns>
        private static string[] SplitMacro(string line)
        {
            return line.Split('<', '>', '(', ',', ')', ';');
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
