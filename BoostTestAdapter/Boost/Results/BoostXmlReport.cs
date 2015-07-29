using System.Globalization;
using System.IO;
using System.Xml.XPath;
using BoostTestAdapter.Boost.Test;

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
        /// Constructor accepting a path to the external file
        /// </summary>
        /// <param name="path">The path to an external file. File will be opened on construction.</param>
        public BoostXmlReport(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Constructor accepting a stream to the file contents
        /// </summary>
        /// <param name="stream">The file content stream.</param>
        public BoostXmlReport(Stream stream)
            : base(stream)
        {
        }

        #endregion Constructors

        #region IBoostOutputParser

        public override void Parse(TestResultCollection collection)
        {
            XPathDocument doc = new XPathDocument(this.InputStream);
            XPathNavigator nav = doc.CreateNavigator();

            // Move to document root node
            if ((nav.MoveToFirstChild()) && (nav.LocalName == Xml.TestResult))
            {
                ParseTestUnitsReport(nav, null, collection);
            }
        }

        #endregion IBoostOutputParser

        /// <summary>
        /// Parses child TestUnit nodes.
        /// </summary>
        /// <param name="nav">The parent XPathNavigator which hosts TestUnit nodes.</param>
        /// <param name="parent">The parent TestSuite to which TestUnits are attached to.</param>
        /// <param name="collection">The TestResultCollection which will host the result.</param>
        private static void ParseTestUnitsReport(XPathNavigator nav, TestSuite parent, TestResultCollection collection)
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
        /// <param name="collection">The TestResultCollection which will host the result.</param>
        private static void ParseTestSuiteReport(XPathNavigator node, TestSuite parent, TestResultCollection collection)
        {
            TestSuite testSuite = new TestSuite(node.GetAttribute(Xml.Name, string.Empty), parent);
            collection[testSuite] = ParseTestResult(node, testSuite, collection);

            ParseTestUnitsReport(node, testSuite, collection);
        }

        /// <summary>
        /// Parses a TestCase node.
        /// </summary>
        /// <param name="node">The XPathNavigator pointing to a TestCase node.</param>
        /// <param name="parent">The parent TestSuite to which TestUnits are attached to.</param>
        /// <param name="collection">The TestResultCollection which will host the result.</param>
        private static void ParseTestCaseReport(XPathNavigator node, TestSuite parent, TestResultCollection collection)
        {
            TestCase testCase = new TestCase(node.GetAttribute(Xml.Name, string.Empty), parent);
            collection[testCase] = ParseTestResult(node, testCase, collection);
        }

        /// <summary>
        /// Parses a general test result information from the provided node.
        /// </summary>
        /// <param name="node">The XPathNavigator pointing to a TestUnit node.</param>
        /// <param name="unit">The test unit for which the test results are related to.</param>
        /// <param name="collection">The TestResultCollection which will host the result.</param>
        private static TestResult ParseTestResult(XPathNavigator node, TestUnit unit, TestResultCollection collection)
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