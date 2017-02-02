// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using NUnit.Framework;

using System.Linq;
using System.Collections.Generic;

using BoostTestAdapter;
using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Discoverers;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;

using BoostTestCase = BoostTestAdapter.Boost.Test.TestCase;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

using BoostTestAdapterNunit.Utility;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    internal class VSDiscoveryVisitorTest
    {
        /// <summary>
        /// Default test source file
        /// </summary>
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
        /// Discovers all tests cases enumerated within the provided framework
        /// </summary>
        /// <param name="framework">The framework to discover tests from</param>
        /// <returns>An enumeration of all the discovered tests</returns>
        private static IEnumerable<VSTestCase> Discover(TestFramework framework)
        {
            var sink = new DefaultTestCaseDiscoverySink();
            var visitor = new VSDiscoveryVisitor(framework.Source, sink);
            framework.MasterTestSuite.Apply(visitor);
            return sink.Tests;
        }

        /// <summary>
        /// Locates the test case with the provided fully qualified name from the provided framework
        /// </summary>
        /// <param name="framework">The framework to locate the test case from</param>
        /// <param name="fullyQualifiedName">The test case fully qualified name to locate</param>
        /// <returns>The located TestCase instance or null if it is not found</returns>
        private static BoostTestCase LocateTestCase(TestFramework framework, string fullyQualifiedName)
        {
            var unit = BoostTestLocator.Locate(framework, fullyQualifiedName);
            return unit as BoostTestCase;
        }

        /// <summary>
        /// Asserts 'common' test case details between the test case encoded in the framework and the respective Visual Studio test case
        /// </summary>
        /// <param name="test">The Visual Studio TestCase instance to verify</param>
        /// <param name="framework">The framework to locate the test case from</param>
        /// <param name="fullyQualifiedName">The test case fully qualified name to locate</param>
        private static void AssertCommonTestCaseDetails(VSTestCase test, TestFramework framework, string fullyQualifiedName)
        {
            Assert.That(framework, Is.Not.Null);

            var expected = LocateTestCase(framework, fullyQualifiedName);

            Assert.That(expected, Is.Not.Null);

            AssertCommonTestCaseDetails(test, expected, framework.Source);
        }

        /// <summary>
        /// Asserts 'common' test case details between the Boost test case and the respective Visual Studio test case
        /// </summary>
        /// <param name="test">The Visual Studio TestCase instance to verify</param>
        /// <param name="expected">The Boost TestCase to which test should match to</param>
        /// <param name="source">The source from which the Boost Test case was discovered from</param>
        private static void AssertCommonTestCaseDetails(VSTestCase test, BoostTestCase expected, string source)
        {
            Assert.That(expected, Is.Not.Null);
            Assert.That(test, Is.Not.Null);

            Assert.That(test.Source, Is.EqualTo(source));
            Assert.That(test.ExecutorUri, Is.EqualTo(new Uri(BoostTestExecutor.ExecutorUriString)));
            Assert.That(test.FullyQualifiedName, Is.EqualTo(expected.FullyQualifiedName));

            if (expected.Source != null)
            {
                Assert.That(test.CodeFilePath, Is.EqualTo(expected.Source.File));
                Assert.That(test.LineNumber, Is.EqualTo(expected.Source.LineNumber));
            }
        }

        /// <summary>
        /// Assert that: Test display names for BOOST_DATA_TEST_CASE instances are different
        ///              than the names determined by the Boost.Test framework
        /// </summary>
        [Test]
        public void BoostDataTestCase()
        {
            TestFramework framework = CreateFrameworkBuilder().
                TestSuite("DataTestCase", 2, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_0", 65536, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_1", 65537, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_2", 65538, new SourceFileInfo(_sourceFile, 10)).
                    TestCase("_3", 65539, new SourceFileInfo(_sourceFile, 10)).
                EndSuite().
                Build();

            var tests = Discover(framework).OrderBy(test => test.FullyQualifiedName).ToList();

            Assert.That(tests.Count, Is.EqualTo(4));

            AssertCommonTestCaseDetails(tests[0], framework, "DataTestCase/_0");
            AssertCommonTestCaseDetails(tests[1], framework, "DataTestCase/_1");
            AssertCommonTestCaseDetails(tests[2], framework, "DataTestCase/_2");
            AssertCommonTestCaseDetails(tests[3], framework, "DataTestCase/_3");

            Assert.That(tests[0].DisplayName, Is.Not.EqualTo(LocateTestCase(framework, "DataTestCase/_0").Name));
            Assert.That(tests[1].DisplayName, Is.Not.EqualTo(LocateTestCase(framework, "DataTestCase/_1").Name));
            Assert.That(tests[2].DisplayName, Is.Not.EqualTo(LocateTestCase(framework, "DataTestCase/_2").Name));
            Assert.That(tests[3].DisplayName, Is.Not.EqualTo(LocateTestCase(framework, "DataTestCase/_3").Name));
        }
    }
}
