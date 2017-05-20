// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using NUnit.Framework;

using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Utility;

using BoostTestAdapterNunit.Utility;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    internal class BoostDataTestCaseVerifierTest
    {
        private const string _sourceFile = @"c:\test.cpp";

        /// <summary>
        /// Creates a 'default' TestFrameworkBuilder instance for this TestFixture
        /// </summary>
        /// <returns>A 'default' TestFrameworkBuilder instance</returns>
        private static TestFrameworkBuilder CreateFrameworkBuilder()
        {
            return new TestFrameworkBuilder("test.exe", "MyTest", 1);
        }

        /// <summary>
        /// Locates the test suite with the provided fully qualified name
        /// </summary>
        /// <param name="framework">The framework from which to locate the test suite</param>
        /// <param name="fullyQualifiedName">The test suite's fully qualified name</param>
        /// <returns>The test suite which was located or null if it was not found</returns>
        private static TestSuite LocateSuite(TestFramework framework, string fullyQualifiedName)
        {
            var suite = BoostTestLocator.Locate(framework, fullyQualifiedName);

            Assert.That(suite, Is.Not.Null);
            Assert.That(suite, Is.TypeOf<TestSuite>());

            return (TestSuite) suite;
        }

        /// <summary>
        /// Verifies if the test suite with the specified fully qualified name is a BOOST_DATA_TEST_CASE
        /// </summary>
        /// <param name="framework">The framework from which to locate the test suite</param>
        /// <param name="fullyQualifiedName">The test suite's fully qualified name</param>
        /// <returns>true if the test suite is a BOOST_DATA_TEST_CASE; false otherwise</returns>
        private static bool IsBoostDataTestCase(TestFramework framework, string fullyQualifiedName)
        {
            var suite = LocateSuite(framework, fullyQualifiedName);
            return (suite != null) && (BoostDataTestCaseVerifier.IsBoostDataTestCase(suite));
        }

        /// <summary>
        /// Assert that: A valid BOOST_DATA_TEST_CASE can be identified as such
        /// </summary>
        [Test]
        public void ValidBoostDataTestCase()
        {
            TestFramework framework = CreateFrameworkBuilder().
                TestSuite("DataTestCase", 2, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_0", 65536, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_1", 65537, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_2", 65538, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_3", 65539, new SourceFileInfo(_sourceFile, 10)).
                EndSuite().
                Build();

            bool result = IsBoostDataTestCase(framework, "DataTestCase");
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Assert that: A test suite containing tests with a pattern similar to BOOST_DATA_TEST_CASE is not identified
        ///              as a data test case instance if the line numbers differ from the parent test suite instance
        /// </summary>
        [Test]
        public void FakeBoostDataTestCase()
        {
            TestFramework framework = CreateFrameworkBuilder().
                TestSuite("BoostUnitTest", 2, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_0", 65536, new SourceFileInfo(_sourceFile, 13)).
                    TestCase("_1", 65537, new SourceFileInfo(_sourceFile, 23)).
                    TestCase("_2", 65538, new SourceFileInfo(_sourceFile, 33)).
                    TestCase("_3", 65539, new SourceFileInfo(_sourceFile, 43)).
                EndSuite().
                Build();

            bool result = IsBoostDataTestCase(framework, "BoostUnitTest");
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Assert that: A regular test suite containing test suites and test cases is not identified as a 
        ///              BOOST_DATA_TEST_CASE
        /// </summary>
        [Test]
        public void RegularTestSuite()
        {
            TestFramework framework = CreateFrameworkBuilder().
                TestSuite("Suite1", 2, new SourceFileInfo(_sourceFile, 10)).
                    TestSuite("Suite2", 3, new SourceFileInfo(_sourceFile, 15)).
                        TestCase("Test1", 65536, new SourceFileInfo(_sourceFile, 23)).
                    EndSuite().
                EndSuite().
                Build();

            {
                bool result = IsBoostDataTestCase(framework, "Suite1");
                Assert.That(result, Is.False);
            }

            {
                bool result = IsBoostDataTestCase(framework, "Suite1/Suite2");
                Assert.That(result, Is.False);
            }
        }

        /// <summary>
        /// Assert that: A regular test suite containing solely test cases is not identified as a 
        ///              BOOST_DATA_TEST_CASE
        /// </summary>
        [Test]
        public void RegularTestCase()
        {
            TestFramework framework = CreateFrameworkBuilder().
                TestSuite("Suite1", 2, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("Test1", 65536, new SourceFileInfo(_sourceFile, 23)).
                EndSuite().
                Build();

            bool result = IsBoostDataTestCase(framework, "Suite1");
            Assert.That(result, Is.False);
        }
    }
}
