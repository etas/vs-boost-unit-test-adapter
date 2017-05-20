// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
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

                FrameworkEqualityVisitor.IsEqualTo(framework, expected, false);
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
            TestFramework expected = new TestFrameworkBuilder(Source, "sample", 1, false).
                TestSuite("suite_1", 2, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 6), false).
                    TestSuite("suite_2", 3, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 8), false).
                        TestCase("test_3", 65536, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 14), null, false).
                        TestCase("test_4", 65537, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 44), null, false).
                        TestCase("test_5", 65538, new SourceFileInfo(@"d:\sample.boostunittest\file_1.cpp", 62), null, false).
                    EndSuite().
                    TestSuite("suite_6", 4, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 19), false).
                        TestCase("test_7", 65539, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 24), null, false).
                        TestCase("test_8", 65540, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 40), null, false).
                        TestCase("test_9", 65541, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 90), null, false).
                        TestCase("test_10", 65542, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 105), null, false).
                        TestCase("test_11", 65543, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 121), null, false).
                        TestCase("test_12", 65544, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 134), null, false).
                        TestCase("test_13", 65545, new SourceFileInfo(@"d:\sample.boostunittest\file_2.cpp", 310), null, false).
                    EndSuite().
                    TestSuite("suite_14", 5, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 20), false).
                        TestCase("test_15", 65546, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 27), null, false).
                        TestCase("test_16", 65547, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 49), null, false).
                        TestCase("test_17", 65548, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 59), null, false).
                        TestCase("test_18", 65549, new SourceFileInfo(@"d:\sample.boostunittest\file_3.cpp", 88), null, false).
                    EndSuite().
                    TestSuite("suite_19", 6, new SourceFileInfo(@"d:\sample.boostunittest\file_4.cpp", 7), false).
                        TestCase("test_20", 65550, new SourceFileInfo(@"d:\sample.boostunittest\file_4.cpp", 12), null, false).
                    EndSuite().
                    TestSuite("suite_21", 7, new SourceFileInfo(@"d:\sample.boostunittest\file_5.cpp", 8), false).
                        TestCase("test_22", 65551, new SourceFileInfo(@"d:\sample.boostunittest\file_5.cpp", 14), null, false).
                    EndSuite().
                    TestSuite("suite_23", 8, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 13), false).
                        TestCase("test_24", 65552, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 18), null, false).
                        TestCase("test_25", 65553, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 28), null, false).
                        TestCase("test_26", 65554, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 38), null, false).
                        TestCase("test_27", 65555, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 48), null, false).
                        TestCase("test_28", 65556, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 59), null, false).
                        TestCase("test_29", 65557, new SourceFileInfo(@"d:\sample.boostunittest\file_6.cpp", 70), null, false).
                    EndSuite().
                    TestSuite("suite_30", 9, new SourceFileInfo(@"d:\sample.boostunittest\file_7.cpp", 8), false).
                        TestCase("test_31", 65558, new SourceFileInfo(@"d:\sample.boostunittest\file_7.cpp", 14), new[] { "hello", "world", "labels" }, false).
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
            TestFramework expected = new TestFrameworkBuilder(Source, "MyTest", 1, false).
                TestCase("Test", 65536, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 30),null, false).
                TestSuite("Suite", 2, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 35), false).
                    TestCase("Test", 65537, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 37), null, false).
                EndSuite().
                TestCase("TestB", 65538, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 44), null, false).
            Build();

            Compare("BoostTestAdapterNunit.Resources.ListContentDOT.sample.3.list.content.gv", expected);
        }
        
        /// <summary>
        /// Deserialisation of Boost Test DOT content consisting of test cases which are
        /// explicitly enabled or disabled using boost decorators
        /// 
        /// Test aims:
        ///     - Ensure that the decorators are correctly read and grouping is done correctly 
        /// </summary>
        [Test]
        public void TestsWithDecorators()
        {
            TestFramework expected = new TestFrameworkBuilder(Source, "Sample", 1).
                TestSuite("Suite1", 2, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 11)).
                    TestCase("MyBoost_Test1", 65536, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 12)).
                    TestCase("MyBoost_Test2", 65537, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 16), null, false).
                    TestCase("MyBoost_Test3", 65538, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 20)).
                    TestCase("MyBoost_Test4", 65539, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 24), null, false).
                EndSuite().
                TestCase("MyBoost_Test5", 65540, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 30)).
                TestCase("MyBoost_Test6", 65541, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 34), null, false).
                TestCase("MyBoost_Test7", 65542, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 38)).
                TestCase("MyBoost_Test8", 65543, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 42), null, false).
            Build();

            Compare("BoostTestAdapterNunit.Resources.ListContentDOT.test_list_content.gv", expected);
        }

        /// <summary>
        /// Assert that: It is possible to deserialize --list_content=DOT output consisting of a BOOST_DATA_TEST_CASE
        /// </summary>
        [Test]
        public void DeserializeBoostDataTestCase()
        {
            TestFramework expected = new TestFrameworkBuilder(Source, "DataTestCaseExample", 1).
                TestSuite("BoostUnitTest", 2, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 10)).
                    TestCase("_0", 65536, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 10)).
                    TestCase("_1", 65537, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 10)).
                    TestCase("_2", 65538, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 10)).
                    TestCase("_3", 65539, new SourceFileInfo(@"c:\boostunittest\boostunittestsample.cpp", 10)).
                EndSuite().
            Build();

            Compare("BoostTestAdapterNunit.Resources.ListContentDOT.boost_data_test_case.gv", expected);
        }
    }
}