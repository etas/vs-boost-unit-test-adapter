// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Diagnostics;

using BoostTestAdapter.Boost.Test;

using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

using TestCase = BoostTestAdapter.Boost.Test.TestCase;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

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
            : this(source, string.Empty, sink)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source test module which contains the discovered tests</param>
        /// <param name="version">The source test module Boost.Test version</param>
        /// <param name="sink">The ITestCaseDiscoverySink which will have tests registered with</param>
        public VSDiscoveryVisitor(string source, string version, ITestCaseDiscoverySink sink)
        {
            Code.Require(sink, "sink");

            this.Source = source;
            this.Version = version;
            this.DiscoverySink = sink;
            this.OutputLog = true;
        }

        /// <summary>
        /// The test module source file path
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// The test module version
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Whether the module should output to the logger regarding relative paths
        /// </summary>
        private bool OutputLog { get; set; }

        /// <summary>
        /// The Visual Studio DiscoverySink which is used to notify test discovery
        /// </summary>
        public ITestCaseDiscoverySink DiscoverySink { get; private set; }
        
        #region ITestVisitor

        public void Visit(TestSuite testSuite)
        {
            Code.Require(testSuite, "testSuite");
            
            if (BoostDataTestCaseVerifier.IsBoostDataTestCase(testSuite))
            {
                foreach (TestUnit child in testSuite.Children)
                {
                    // NOTE Since we have asserted that the suite is a BOOST_DATA_TEST_CASE,
                    //      all child instances are to be of type TestCase

                    var displayName = testSuite.Name + '/' + child.Name;
                    Visit((TestCase)child, displayName);
                }
            }
            else
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

            Visit(testCase, testCase.Name);
        }

        #endregion ITestVisitor

        /// <summary>
        /// Visits the provided TestCase
        /// </summary>
        /// <param name="testCase">The TestCase which is to be visited</param>
        /// <param name="displayName">The test case display name to use (overrides the test case name)</param>
        private void Visit(TestCase testCase, string displayName)
        {
            Code.Require(testCase, "testCase");

            VSTestCase test = GenerateTestCase(testCase);
            test.DisplayName = string.IsNullOrEmpty(displayName) ? test.DisplayName : displayName;

            // Send to discovery sink
            Logger.Info("Found test: {0}", test.FullyQualifiedName);

            this.DiscoverySink.SendTestCase(test);
        }

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

            // Record Boost version if available
            if (!string.IsNullOrEmpty(this.Version))
            {
                test.SetPropertyValue(VSTestModel.VersionProperty, this.Version);
            }

            return test;
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
