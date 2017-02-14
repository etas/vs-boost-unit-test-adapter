// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace BoostTestAdapter.Utility.VisualStudio
{
    /// <summary>
    /// An ITestCaseDiscoverySink implementation. Aggregates all tests
    /// within an internal collection which is publicly accessible.
    /// </summary>
    public class DefaultTestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        private ICollection<TestCase> _tests = new HashSet<TestCase>(new TestCaseComparer());

        /// <summary>
        /// The collection of discovered TestCases
        /// </summary>
        public IEnumerable<TestCase> Tests
        {
            get
            {
                return _tests;
            }
        }

        #region ITestCaseDiscoverySink

        public void SendTestCase(TestCase discoveredTest)
        {
            this._tests.Add(discoveredTest);
        }

        #endregion ITestCaseDiscoverySink
    }
    
    /// <summary>
    /// TestCase equality comparer which defines equality based on the TestCase's
    /// Fully Qualified Name.
    /// </summary>
    public class TestCaseComparer : IEqualityComparer<TestCase>
    {
        #region IEqualityComparer<TestCase>

        public bool Equals(TestCase x, TestCase y)
        {
            Utility.Code.Require(x, "x");
            Utility.Code.Require(y, "y");

            return (x.FullyQualifiedName == y.FullyQualifiedName) && (x.Source == y.Source);
        }

        public int GetHashCode(TestCase obj)
        {
            Utility.Code.Require(obj, "obj");

            return obj.FullyQualifiedName.GetHashCode() ^ obj.Source.GetHashCode();
        }

        #endregion IEqualityComparer<TestCase>
    }
}