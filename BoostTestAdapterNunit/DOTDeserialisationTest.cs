// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;
using BoostTestAdapter.Boost.Test;

using BoostTestAdapterNunit.Utility;

using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class DOTDeserialisationTest
    {
        #region Test Data

        /// <summary>
        /// Default source identifier for test purposes
        /// </summary>
        private static string Source
        {
            get
            {
                return "source.exe";
            }
        }

        #endregion Test Data

        #region Helper Methods

        /// <summary>
        /// Compares a test framework deserialised from the provided embedded resource against the expected one
        /// </summary>
        /// <param name="resource">Path to an embedded resource which is to be treated as input for TestFramework deserialisation</param>
        /// <param name="expected">The TestFramework which the deserialised version should be compared against</param>
        private void Compare(string resource, TestFramework expected)
        {
            using (var stream = TestHelper.LoadEmbeddedResource(resource))
            {
                TestFrameworkDOTDeserialiser parser = new TestFrameworkDOTDeserialiser(Source);
                TestFramework framework = parser.Deserialise(stream);

                FrameworkEqualityVisitor.IsEqualTo(framework, expected);
            }
        }

        #endregion Helper Methods

        /// <summary>
        /// Deserialisation of (relatively large) Boost Test DOT content containing most features
        /// 
        /// Test aims:
        ///     - Ensure that a TestFramework can be deserialised correctly from DOT content
        /// </summary>
        [Test]
        public void DeserialiseDOT()
        {
            TestFramework expected = new TestFrameworkBuilder(Source, "sample", 1).
                TestSuite("suite_1", 2, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 6)).
                    TestSuite("suite_2", 3, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 8)).
                        TestCase("test_3", 65536, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 14)).
                        TestCase("test_4", 65537, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 44)).
                        TestCase("test_5", 65538, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 62)).
                    EndSuite().
                    TestSuite("suite_6", 4, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 19)).
                        TestCase("test_7", 65539, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 24)).
                        TestCase("test_8", 65540, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 40)).
                        TestCase("test_9", 65541, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 90)).
                        TestCase("test_10", 65542, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 405)).
                        TestCase("test_11", 65543, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 121)).
                        TestCase("test_12", 65544, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 134)).
                        TestCase("test_13", 65545, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 310)).
                    EndSuite().
                    TestSuite("suite_14", 5, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 19)).
                        TestCase("test_15", 65546, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 27)).
                        TestCase("test_16", 65547, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 49)).
                        TestCase("test_17", 65548, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 54)).
                        TestCase("test_18", 65549, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 88)).
                    EndSuite().
                    TestSuite("suite_19", 6, new SourceFileInfo(@"d:\sample.boostunittest\file_4.cpp", 7)).
                        TestCase("test_20", 65550, new SourceFileInfo(@"d:\sample.boostunittest\file_4.cpp", 12)).
                    EndSuite().
                    TestSuite("suite_21", 7, new SourceFileInfo(@"d:\sample.boostunittest\file_5.cpp", 8)).
                        TestCase("test_22", 65551, new SourceFileInfo(@"d:\sample.boostunittest\file_5.cpp", 14)).
                    EndSuite().
                    TestSuite("suite_23", 8, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 13)).
                        TestCase("test_24", 65552, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 18)).
                        TestCase("test_25", 65553, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 28)).
                        TestCase("test_26", 65554, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 38)).
                        TestCase("test_27", 65555, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 48)).
                        TestCase("test_28", 65556, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 59)).
                        TestCase("test_29", 65557, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 70)).
                    EndSuite().
                    TestSuite("suite_30", 9, new SourceFileInfo(@"d:\sample.boostunittest\file_7.cpp", 8)).
                        TestCase("test_31", 65558, new SourceFileInfo(@"d:\sample.boostunittest\file_7.cpp", 14), new[] { "hello", "world", "labels" }).
                    EndSuite().
                EndSuite().
            Build();

            Compare("BoostTestAdapterNunit.Resources.ListContentDOT.sample.list.content.gv", expected);
        }

        /// <summary>
        /// Deserialisation of Boost Test DOT content consisting of the following scenario:
        /// 
        /// - Master Test Suite
        /// -- Test Case
        /// -- Test Suite
        /// --- Test Case
        /// -- Test Case
        /// 
        /// Test aims:
        ///     - Ensure that a TestFramework can be deserialised correctly from DOT content
        /// </summary>
        [Test]
        public void TestSuiteWithChildTestsAndSuites()
        {
            TestFramework expected = new TestFrameworkBuilder(Source, "MyTest", 1).
                TestCase("Test", 65536, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 30)).
                TestSuite("Suite", 2, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 35)).
                    TestCase("Test", 65537, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 37)).
                EndSuite().
                TestCase("TestB", 65538, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 44)).
            Build();

            Compare("BoostTestAdapterNunit.Resources.ListContentDOT.sample.3.list.content.gv", expected);
        }
    }
}
