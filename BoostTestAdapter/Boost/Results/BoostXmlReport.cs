// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using System.Collections.Generic;
using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Boost Xml Report
    /// </summary>
    public class BoostXmlReport : BoostTestResultXMLOutput
    {
        #region Constants

        /// <summary>
        /// Xml constants
        /// </summary>
        private static class Xml
        {
            public const string TestResult = "TestResult";
            public const string TestSuite = "TestSuite";
            public const string TestCase = "TestCase";
            public const string Name = "name";
            public const string Result = "result";
            public const string AssertionsPassed = "assertions_passed";
            public const string AssertionsFailed = "assertions_failed";
            public const string ExpectedFailures = "expected_failures";
        }

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The destination result collection. Possibly used for result aggregation.</param>
        public BoostXmlReport(IDictionary<string, TestResult> target)
            : base(target)
        {
        }

        #endregion Constructors

        #region BoostTestResultXMLOutput

        protected override IDictionary<string, TestResult> ParseXml(string xml)
        {
            using (var reader = new StringReader(xml))
            {
                XPathDocument doc = new XPathDocument(reader);
                XPathNavigator nav = doc.CreateNavigator();

                // Move to document root node
                if ((nav.MoveToFirstChild()) && (nav.LocalName == Xml.TestResult))
                {
                    ParseTestUnitsReport(nav, null, Target);
                }

                return Target;
            }
        }

        #endregion BoostTestResultXMLOutput

        /// <summary>
        /// Parses child TestUnit nodes.
        /// </summary>
        /// <param name="nav">The parent XPathNavigator which hosts TestUnit nodes.</param>
        /// <param name="parent">The parent TestSuite to which TestUnits are attached to.</param>
        /// <param name="collection">The test result collection which will host the result.</param>
        private static void ParseTestUnitsReport(XPathNavigator nav, TestSuite parent, IDictionary<string, TestResult> collection)
        {
            foreach (XPathNavigator child in nav.SelectChildren(Xml.TestSuite, string.Empty))
            {
                ParseTestSuiteReport(child, parent, collection);
            }

            foreach (XPathNavigator child in nav.SelectChildren(Xml.TestCase, string.Empty))
            {
                ParseTestCaseReport(child, parent, collection);
            }
        }

        /// <summary>
        /// Parses a TestSuite node.
        /// </summary>
        /// <param name="node">The XPathNavigator pointing to a TestSuite node.</param>
        /// <param name="parent">The parent TestSuite to which TestUnits are attached to.</param>
        /// <param name="collection">The test result collection which will host the result.</param>
        private static void ParseTestSuiteReport(XPathNavigator node, TestSuite parent, IDictionary<string, TestResult> collection)
        {
            TestSuite testSuite = new TestSuite(node.GetAttribute(Xml.Name, string.Empty), parent);
            collection[testSuite.FullyQualifiedName] = ParseTestResult(node, testSuite, collection);

            ParseTestUnitsReport(node, testSuite, collection);
        }

        /// <summary>
        /// Parses a TestCase node.
        /// </summary>
        /// <param name="node">The XPathNavigator pointing to a TestCase node.</param>
        /// <param name="parent">The parent TestSuite to which TestUnits are attached to.</param>
        /// <param name="collection">The test result collection which will host the result.</param>
        private static void ParseTestCaseReport(XPathNavigator node, TestSuite parent, IDictionary<string, TestResult> collection)
        {
            QualifiedNameBuilder fullname = new QualifiedNameBuilder(parent);
            fullname.Push(node.GetAttribute(Xml.Name, string.Empty));

            TestCase testCase = null;

            // If the test is already available, reuse it
            TestResult current = null;
            if (collection.TryGetValue(fullname.ToString(), out current))
            {
                testCase = current.Unit as TestCase;
            }

            // Else construct and add it to the appropriate parent
            if (testCase == null)
            {
                testCase = new TestCase(fullname.Peek(), parent);
            }

            TestResult result = ParseTestResult(node, testCase, collection);

            // Aggregate results. Common use-case in BOOST_DATA_TEST_CASE.
            collection[fullname.ToString()] = Aggregate(result, current);
        }

        /// <summary>
        /// Aggregates the two results as one result structure if compatible.
        /// </summary>
        /// <param name="lhs">The left-hand side result to aggregate</param>
        /// <param name="rhs">The right-hand side result to aggregate</param>
        /// <returns>A TestResult instance consisting of both results as one or lhs in case of incompatibilities</returns>
        private static TestResult Aggregate(TestResult lhs, TestResult rhs)
        {
            // If lhs and rhs are incompatible, return the first non-null argument
            if ((lhs == null) || (rhs == null) || (lhs.Collection != rhs.Collection) || (lhs.Unit.FullyQualifiedName != rhs.Unit.FullyQualifiedName))
            {
                return ((lhs != null) ? lhs : rhs);
            }

            TestResult rvalue = new TestResult(lhs.Collection);
            rvalue.Unit = lhs.Unit;

            // Select the worst of the result types
            int result = Math.Max((int) lhs.Result, (int) rhs.Result);
            rvalue.Result = (TestResultType) result;

            // Sum up totals
            rvalue.AssertionsPassed = lhs.AssertionsPassed + rhs.AssertionsPassed;
            rvalue.AssertionsFailed = lhs.AssertionsFailed + rhs.AssertionsFailed;
            rvalue.ExpectedFailures = lhs.ExpectedFailures + rhs.ExpectedFailures;

            return rvalue;
        }

        /// <summary>
        /// Parses a general test result information from the provided node.
        /// </summary>
        /// <param name="node">The XPathNavigator pointing to a TestUnit node.</param>
        /// <param name="unit">The test unit for which the test results are related to.</param>
        /// <param name="collection">The test result collection which will host the result.</param>
        private static TestResult ParseTestResult(XPathNavigator node, TestUnit unit, IDictionary<string, TestResult> collection)
        {
            TestResult result = new TestResult(collection);

            result.Unit = unit;
            result.Result = ParseResultType(node.GetAttribute(Xml.Result, string.Empty));

            result.AssertionsPassed = uint.Parse(node.GetAttribute(Xml.AssertionsPassed, string.Empty), CultureInfo.InvariantCulture);
            result.AssertionsFailed = uint.Parse(node.GetAttribute(Xml.AssertionsFailed, string.Empty), CultureInfo.InvariantCulture);
            result.ExpectedFailures = uint.Parse(node.GetAttribute(Xml.ExpectedFailures, string.Empty), CultureInfo.InvariantCulture);

            return result;
        }

        /// <summary>
        /// Parses a Result enumeration from string.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed Result enumeration value.</returns>
        private static TestResultType ParseResultType(string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "PASSED": return TestResultType.Passed;
                case "FAILED": return TestResultType.Failed;
                case "ABORTED": return TestResultType.Aborted;
                case "SKIPPED": return TestResultType.Skipped;
            }

            return TestResultType.Skipped;
        }
    }
}