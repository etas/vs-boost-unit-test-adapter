// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;

using BoostTestAdapter.Utility;
using BoostTestAdapter.Boost.Test;

using NUnit.Framework;
using System.Collections.Generic;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// An ITestVisitor implementation which ensures that 2 test unit hierarchies are equal
    /// </summary>
    public class FrameworkEqualityVisitor : ITestVisitor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expected">The expected root test unit hierarchy which is to be compared against</param>
        private FrameworkEqualityVisitor(TestUnit expected, bool respectOrder = true)
        {
            this.ExpectedUnit = expected;
            this.OrderRespected = respectOrder;
        }

        /// <summary>
        /// The expected test unit
        /// </summary>
        public TestUnit Expected { get; private set; }

        /// <summary>
        /// Used to record which test unit is currently under test
        /// </summary>
        private TestUnit ExpectedUnit { get; set; }

        /// <summary>
        /// States if test unit ordering is respected
        /// </summary>
        public bool OrderRespected { get; private set; }

        #region ITestVisitor

        public void Visit(TestSuite testSuite)
        {
            Assert.That(this.ExpectedUnit, Is.TypeOf<TestSuite>());
            Assert.That(testSuite.Children.Count(), Is.EqualTo(this.ExpectedUnit.Children.Count()));

            VerifyTestUnit(testSuite, this.ExpectedUnit);

            var expectedChild = Sort(this.ExpectedUnit.Children).GetEnumerator();

            foreach (TestUnit child in Sort(testSuite.Children))
            {
                expectedChild.MoveNext();
                this.ExpectedUnit = expectedChild.Current;
                child.Apply(this);
                this.ExpectedUnit = this.ExpectedUnit.Parent;
            }
        }

        public void Visit(TestCase testCase)
        {
            Assert.That(this.ExpectedUnit, Is.TypeOf<TestCase>());
            Assert.That(testCase.Children, Is.Empty);

            VerifyTestUnit(testCase, this.ExpectedUnit);
        }

        #endregion ITestVisitor

        private IEnumerable<TestUnit> Sort(IEnumerable<TestUnit> tests)
        {
            return (this.OrderRespected) ? tests : tests.OrderBy(unit => unit.Id);
        }

        /// <summary>
        /// Verifies that both test units are equal
        /// </summary>
        /// <param name="actual">The actual TestUnit to be compared against</param>
        /// <param name="expected">The expected TestUnit 'actual' should match</param>
        private static void VerifyTestUnit(TestUnit actual, TestUnit expected)
        {
            Assert.That(actual.Name, Is.EqualTo(expected.Name));
            Assert.That(actual.Id, Is.EqualTo(expected.Id));
            Assert.That(actual.Labels, Is.EquivalentTo(expected.Labels));

            if (expected.Source != null)
            {
                VerifySoureFileInfo(actual.Source, expected.Source);
            }
            else
            {
                Assert.That(actual.Source, Is.Null);
            }
        }

        /// <summary>
        /// Verifies that both source file information are equal
        /// </summary>
        /// <param name="actual">The actual SourceFileInfo to be compared against</param>
        /// <param name="expected">The expected SourceFileInfo 'actual' should match</param>
        private static void VerifySoureFileInfo(SourceFileInfo actual, SourceFileInfo expected)
        {
            Assert.That(actual.File, Is.EqualTo(expected.File));
            Assert.That(actual.LineNumber, Is.EqualTo(expected.LineNumber));
        }

        /// <summary>
        /// Asserts TestFramework equality i.e. both test hierarchies are equivalent in test units
        /// </summary>
        /// <param name="actual">The actual TestFramework to be compared against</param>
        /// <param name="expected">The expected TestFramework 'actual' should match</param>
        public static void IsEqualTo(TestFramework actual, TestFramework expected, bool respectOrder = true)
        {
            Assert.That(actual.Source, Is.EqualTo(expected.Source));

            if (actual.MasterTestSuite == null)
            {
                Assert.That(expected.MasterTestSuite, Is.Null);
            }
            else
            {
                actual.MasterTestSuite.Apply(new FrameworkEqualityVisitor(expected.MasterTestSuite, respectOrder));
            }
        }
    }
}
