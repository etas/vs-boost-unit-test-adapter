// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Diagnostics;

using BoostTestAdapter.Boost.Test;

using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

using TestCase = BoostTestAdapter.Boost.Test.TestCase;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using System.IO;

namespace BoostTestAdapter.Discoverers
{
    /// <summary>
    /// ITestVisitor implementation. Visits TestCases and registers them
    /// with the supplied ITestCaseDiscoverySink.
    /// </summary>
    public class VSDiscoveryVisitor : ITestVisitor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source test module which contains the discovered tests</param>
        /// <param name="sink">The ITestCaseDiscoverySink which will have tests registered with</param>
        public VSDiscoveryVisitor(string source, ITestCaseDiscoverySink sink)
        {
            this.Source = source;
            this.DiscoverySink = sink;
            this.OutputLog = true;
        }

        /// <summary>
        /// The test module source file path
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Whether the module should output to the logger regarding relative paths
        /// </summary>
        public bool OutputLog { get; private set; }

        /// <summary>
        /// The Visual Studio DiscoverySink which is used to notify test discovery
        /// </summary>
        public ITestCaseDiscoverySink DiscoverySink { get; private set; }
        
        #region ITestVisitor

        public void Visit(TestSuite testSuite)
        {
            Code.Require(testSuite, "testSuite");

            if (ShouldVisit(testSuite))
            {
                foreach (TestUnit child in testSuite.Children)
                {
                    child.Apply(this);
                }
            }
        }

        public void Visit(TestCase testCase)
        {
            Code.Require(testCase, "testCase");

            if (ShouldVisit(testCase))
            {
                VSTestCase test = GenerateTestCase(testCase);

                // Send to discovery sink
                if (null != this.DiscoverySink)
                {
                    Logger.Info("Found test: {0}", test.FullyQualifiedName);
                    this.DiscoverySink.SendTestCase(test);
                }
            }
        }

        #endregion ITestVisitor

        /// <summary>
        /// Generates a Visual Studio equivalent test case structure.
        /// </summary>
        /// <param name="testCase">The Boost.Test.TestCase to convert.</param>
        /// <returns>An equivalent Visual Studio TestCase structure to the one provided.</returns>
       
        private VSTestCase GenerateTestCase(TestCase testCase)
        {
            VSTestCase test = new VSTestCase(
                testCase.FullyQualifiedName,
                BoostTestExecutor.ExecutorUri,
                this.Source
            );

            test.DisplayName = testCase.Name;
            
            if (testCase.Source != null)
            {
                // NOTE As of Boost 1.61, this warning might be triggered when BOOST_DATA_TEST_CASEs are used due to irregular DOT output
                if (!Path.IsPathRooted(testCase.Source.File) && this.OutputLog)
                {
                    Logger.Info("Relative Paths are being used. Please note that test navigation from the Test Explorer window will not be available. To enable such functionality, the Use Full Paths setting under C++ -> Advanced in the project's Property Page must be set to Yes (/FC).");
                    this.OutputLog = false;
                }

                test.CodeFilePath = testCase.Source.File;
                test.LineNumber = testCase.Source.LineNumber;
            }

            // Register the test suite as a trait
            test.Traits.Add(new Trait(VSTestModel.TestSuiteTrait, GetParentFullyQualifiedName(testCase)));
            
            // Register enabled and disabled as traits
            test.Traits.Add(new Trait(VSTestModel.StatusTrait, (testCase.DefaultEnabled ? VSTestModel.TestEnabled : VSTestModel.TestDisabled)));

            TestUnit unit = testCase;
            while (unit != null)
            {
                foreach (string label in unit.Labels)
                {
                    // Register each and every label as an individual trait
                    test.Traits.Add(new Trait(label, string.Empty));
                }             

                // Test cases inherit the labels of parent test units
                // Reference: http://www.boost.org/doc/libs/1_60_0/libs/test/doc/html/boost_test/tests_organization/tests_grouping.html
                unit = unit.Parent;
            }

            return test;
        }

        /// <summary>
        /// States whether the provided test suite should be visited
        /// </summary>
        /// <param name="unit">The test suite under consideration</param>
        /// <returns>true if the provided TestSuite should be visited; false otherwise</returns>
        protected virtual bool ShouldVisit(TestSuite suite)
        {
            return true;
        }

        /// <summary>
        /// States whether the provided test case should be visited
        /// </summary>
        /// <param name="unit">The test case under consideration</param>
        /// <returns>true if the provided TestCase should be visited; false otherwise</returns>
        protected virtual bool ShouldVisit(TestCase test)
        {
            return true;
        }

        /// <summary>
        /// Provides the fully qualified name of the parent TestUnit of the provided TestCase
        /// </summary>
        /// <param name="test">The TestCase whose parent TestUnit is to be queried</param>
        /// <returns>The fully qualified name of the parent TestUnit</returns>
        private static string GetParentFullyQualifiedName(TestCase test)
        {
            Code.Require(test, "test");

            TestUnit parent = test.Parent;

            // A test case must have a parent, at the very least, the master test suite should be the parent of a test case
            Debug.Assert(parent != null);

            // Since the master test suite name is not included in the fully qualified name, identify
            // this edge case and explicitly return the master test suite name in such cases.
            if (parent.Parent == null)
            {
                return string.IsNullOrEmpty(parent.Name) ? QualifiedNameBuilder.DefaultMasterTestSuiteName : parent.Name;
            }

            return parent.FullyQualifiedName;
        }
    }
}
