// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Utility;
using NUnit.Framework;
using BoostTestAdapterNunit.Utility;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestTest
    {
        #region Test Data

        /// <summary>
        /// Default test source
        /// </summary>
        private const string Source = @"C:\tests.dll";

        #endregion Test Data
        
        #region Helper Methods
        
        /// <summary>
        /// Looks up a test unit by fully qualified name
        /// </summary>
        /// <param name="root">The root test unit from which to start searching</param>
        /// <param name="fullyQualifiedName">The fully qualified name of the test unit to look for</param>
        /// <returns>The test unit with the requested fully qualified name or null if it cannot be found</returns>
        private TestUnit Lookup(TestUnit root, string fullyQualifiedName)
        {
            return BoostTestLocator.Locate(root, fullyQualifiedName);
        }
        
        /// <summary>
        /// Asserts test unit details
        /// </summary>
        /// <param name="unit">The test unit to test</param>
        /// <param name="type">The expected type of the test unit</param>
        /// <param name="id">The expected Id of the test unit</param>
        /// <param name="parent">The expected parent of the test unit</param>
        private void AssertTestUnit(TestUnit unit, Type type, int id, TestUnit parent)
        {
            Assert.That(unit, Is.Not.Null);
            Assert.That(unit, Is.TypeOf(type));
            Assert.That(unit.Id, Is.EqualTo(id));
            Assert.That(unit.Parent, Is.EqualTo(parent));
        }

        /// <summary>
        /// Asserts test suite details
        /// </summary>
        /// <param name="unit">The test suite to test</param>
        /// <param name="id">The expected Id of the test suite</param>
        /// <param name="parent">The expected parent of the test suite</param>
        private void AssertTestSuite(TestUnit unit, int id, TestUnit parent)
        {
            AssertTestUnit(unit, typeof(TestSuite), id, parent);
        }

        /// <summary>
        /// Asserts test case details
        /// </summary>
        /// <param name="unit">The test case to test</param>
        /// <param name="id">The expected Id of the test case</param>
        /// <param name="info">The expected source file information of the test case</param>
        /// <param name="parent">The expected parent of the test case</param>
        private void AssertTestCase(TestUnit unit, int id, SourceFileInfo info, TestUnit parent)
        {
            AssertTestUnit(unit, typeof(TestCase), id, parent);

            TestCase test = ((TestCase) unit);

            Assert.That(test.Children, Is.Empty);

            SourceFileInfo unitInfo = test.Source;

            if (info == null)
            {
                Assert.That(unitInfo, Is.Null);
            }
            else
            {
                Assert.That(unitInfo.File, Is.EqualTo(info.File));
                Assert.That(unitInfo.LineNumber, Is.EqualTo(info.LineNumber));
            }
        }

        #endregion Helper Methods

        #region Tests

        /// <summary>
        /// Test cases cannot be parents of other test units.
        /// 
        /// Test aims:
        ///     - Ensure that TestCases cannot accept other test units as children.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddChildToTestCase()
        {
            TestSuite master = new TestSuite("master", null);
            TestCase test = new TestCase("test", master);

            test.AddChild(new TestCase());
        }

        /// <summary>
        /// Tests can be uniquely identified via their qualified name.
        /// 
        /// Test aims:
        ///     - Tests with similar names can still be distinctly identified based on their fully qualified name.
        /// </summary>
        [Test]
        public void TestQualifiedNamingScheme()
        {
            TestFramework framework = new TestFrameworkBuilder(Source, "Master Test Suite", 1).
                TestCase("test", 2).
                TestSuite("suite", 3).
                    TestSuite("suite", 4).
                        TestCase("test", 5).
                    EndSuite().
                EndSuite().
                Build();

            // Master Test Suite fully qualified name is equivalent to the empty string
            Assert.That(framework.MasterTestSuite.FullyQualifiedName, Is.Empty);

            // Test Units which fall directly under the Master Test Suite will
            // have their (local) name equivalent to their fully qualified name
            foreach (TestUnit child in framework.MasterTestSuite.Children)
            {
                Assert.That(child.FullyQualifiedName, Is.EqualTo(child.Name));
            }

            // Test Fully Qualified Name scheme via lookup

            TestUnit test = Lookup(framework.MasterTestSuite, "test");
            AssertTestCase(test, 2, null, framework.MasterTestSuite);

            TestUnit suite = Lookup(framework.MasterTestSuite, "suite");
            AssertTestSuite(suite, 3, framework.MasterTestSuite);

            TestUnit suiteSuite = Lookup(framework.MasterTestSuite, "suite/suite");
            AssertTestSuite(suiteSuite, 4, suite);

            TestUnit suiteSuiteTest = Lookup(framework.MasterTestSuite, "suite/suite/test");
            AssertTestCase(suiteSuiteTest, 5, null, suiteSuite);
        }

        #endregion Tests
    }
}