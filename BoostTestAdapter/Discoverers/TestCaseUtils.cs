// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace BoostTestAdapter.Discoverers
{
    /// <summary>
    /// Static methods used by all IBoostTestDiscoverer
    /// </summary>
    class TestCaseUtils
    {
        /// <summary>
        /// Creates a new TestCase object.
        /// </summary>
        /// <param name="sourceExe">Name of the project executable</param>
        /// <param name="sourceInfo">.cpp file path and TestCase line number</param>
        /// <param name="suite">The suite in which testcase is present</param>
        /// <param name="testCaseName">Name of the testcase</param>
        /// <param name="isEnabled">The enabling status of the testcase</param>
        /// <returns>The created TestCase object</returns>
        public static TestCase CreateTestCase(string sourceExe, SourceFileInfo sourceInfo, QualifiedNameBuilder suite, string testCaseName, bool isEnabled = true)
        {
            suite.Push(testCaseName);

            string qualifiedName = suite.ToString();

            suite.Pop();

            var testCase = new TestCase(qualifiedName, BoostTestExecutor.ExecutorUri, sourceExe)
            {
                CodeFilePath = sourceInfo.File,
                LineNumber = sourceInfo.LineNumber,
                DisplayName = testCaseName,
            };

            GroupViaTraits(suite.ToString(), testCase, isEnabled);

            return testCase;
        }

        /// <summary>
        /// Sets the Traits property for the testcase object.
        /// </summary>
        /// <param name="suiteName">Name of the test suite to which the testcase belongs</param>
        /// <param name="testCase">[ref] The testcase object</param>
        /// <param name="isEnabled">The enabling status of the testcase</param>
        private static void GroupViaTraits(string suiteName, TestCase testCase, bool isEnabled = true)
        {
            string traitName = suiteName;

            if (string.IsNullOrEmpty(suiteName))
            {
                traitName = QualifiedNameBuilder.DefaultMasterTestSuiteName;
            }
            if (isEnabled)
            {
                testCase.Traits.Add(VSTestModel.TestSuiteTrait, traitName);
            }
            else
            {
                testCase.Traits.Add(VSTestModel.DisabledTestSuiteTrait, traitName);
            };
        }

        /// <summary>
        /// Helper methods which adds a test case to an internal list and sends the test to the discovery sink
        /// </summary>
        /// <param name="testCase">the test case to be added</param>
        /// <param name="discoverySink">the discovery sink where the test case is sent to</param>
        public static void AddTestCase(TestCase testCase, ITestCaseDiscoverySink discoverySink)
        {
            //send to discovery sink
            if (null != discoverySink)
            {
                Logger.Info("Found test: {0}", testCase.FullyQualifiedName);
                discoverySink.SendTestCase(testCase);
            }
        }

    }
}
