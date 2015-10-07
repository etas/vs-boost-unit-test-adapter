// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter.Boost.Results.LogEntryTypes;
using BoostTestAdapter.Boost.Test;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Test result enumeration.
    /// </summary>
    public enum TestResultType
    {
        Passed,
        Skipped,
        Aborted,
        Failed
    }

    /// <summary>
    /// Aggregates Boost Test result information.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TestResult() :
            this(null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="collection">The parent collection which hosts this TestResult.</param>
        public TestResult(TestResultCollection collection)
        {
            this.Collection = collection;
            this.LogEntries = new List<LogEntry>();
        }

        /// <summary>
        /// Parent collection which hosts this result.
        /// </summary>
        public TestResultCollection Collection { get; private set; }

        /// <summary>
        /// Test Unit related to this test result.
        /// </summary>
        public TestUnit Unit { get; set; }

        /// <summary>
        /// Result type.
        /// </summary>
        public TestResultType Result { get; set; }

        /// <summary>
        /// Number of assertions passed.
        /// </summary>
        public uint AssertionsPassed { get; set; }

        /// <summary>
        /// Number of assertions failed.
        /// </summary>
        public uint AssertionsFailed { get; set; }

        /// <summary>
        /// Number of expected failures.
        /// </summary>
        public uint ExpectedFailures { get; set; }

        /// <summary>
        /// Number of contained test cases which passed.
        /// </summary>
        public uint TestCasesPassed
        {
            get
            {
                return GetCount(TestResultType.Passed);
            }
        }

        /// <summary>
        /// Number of contained test cases which failed.
        /// </summary>
        public uint TestCasesFailed
        {
            get
            {
                return GetCount(new TestResultType[] { TestResultType.Failed, TestResultType.Aborted });
            }
        }

        /// <summary>
        /// Number of contained test cases which were skipped.
        /// </summary>
        public uint TestCasesSkipped
        {
            get
            {
                return GetCount(TestResultType.Skipped);
            }
        }

        /// <summary>
        /// Number of contained test cases which were aborted.
        /// </summary>
        public uint TestCasesAborted
        {
            get
            {
                return GetCount(TestResultType.Aborted);
            }
        }

        /// <summary>
        /// Duration of test in microseconds
        /// </summary>
        public ulong Duration { get; set; }

        /// <summary>
        /// Collection of related log entries.
        /// </summary>
        public ICollection<LogEntry> LogEntries { get; private set; }

        #region Utility

        /// <summary>
        /// Returns the number of contained test cases which are of the specified Result type.
        /// </summary>
        /// <param name="type">The result type to lookup</param>
        /// <returns>The number of contained test cases which are of the specified Result type.</returns>
        private uint GetCount(TestResultType type)
        {
            return GetCount(new TestResultType[] { type });
        }

        /// <summary>
        /// Returns the number of contained test cases which are of the specified Result type.
        /// </summary>
        /// <param name="types">The result types to lookup</param>
        /// <returns>The number of contained test cases which are of the specified Result type.</returns>
        private uint GetCount(IEnumerable<TestResultType> types)
        {
            if (this.Collection == null)
            {
                if (this.Unit is TestCase)
                {
                    return (types.Contains(this.Result)) ? 1u : 0u;
                }
            }
            else
            {
                TestCaseResultVisitor visitor = new TestCaseResultVisitor(this.Collection, types);
                this.Unit.Apply(visitor);
                return visitor.Count;
            }

            return 0u;
        }

        /// <summary>
        /// Boost Test Unit visitor implementation. Aggregates the number of
        /// test cases which are of a specific Result type.
        /// </summary>
        private class TestCaseResultVisitor : ITestVisitor
        {
            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="collection">The TestResultCollection which hosts all results.</param>
            /// <param name="types">The types to lookup</param>
            public TestCaseResultVisitor(TestResultCollection collection, IEnumerable<TestResultType> types)
            {
                this.Collection = collection;
                this.ResultTypes = types;
                this.Count = 0;
            }

            #endregion Constructors

            #region Properties

            public TestResultCollection Collection { get; private set; }

            public IEnumerable<TestResultType> ResultTypes { get; private set; }

            /// <summary>
            /// The number of test cases encountered which are of the specified Result type.
            /// </summary>
            public uint Count { get; private set; }

            #endregion Properties

            #region ITestVisitor

            public void Visit(TestCase testCase)
            {
                Utility.Code.Require(testCase, "testCase");

                TestResult result = this.Collection[testCase];

                if ((result != null) && (this.ResultTypes.Contains(result.Result)))
                {
                    ++this.Count;
                }
            }

            public void Visit(TestSuite testSuite)
            {
                Utility.Code.Require(testSuite, "testSuite");

                foreach (TestUnit unit in testSuite.Children)
                {
                    unit.Apply(this);
                }
            }

            #endregion ITestVisitor
        }

        #endregion Utility
    }
}