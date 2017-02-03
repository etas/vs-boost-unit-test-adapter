// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Text.RegularExpressions;

using BoostTestAdapter.Boost.Test;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Utility class which determines if a test suite is, in fact, a BOOST_DATA_TEST_CASE
    /// </summary>
    public class BoostDataTestCaseVerifier : ITestVisitor
    {
        /// <summary>
        /// The test case name pattern used for BOOST_DATA_TEST_CASE instances
        /// </summary>
        private static readonly Regex _dataTestCaseNamePattern = new Regex(@"^_\d+$");

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="testSuite">The test suite which is to be determined if it is a BOOST_DATA_TEST_CASE</param>
        private BoostDataTestCaseVerifier(TestSuite testSuite)
        {
            this.RootTestSuite = testSuite;
            this.DataTestCase = true;
        }

        /// <summary>
        /// The root test suite which is to be determined if it is a BOOST_DATA_TEST_CASE
        /// </summary>
        public TestSuite RootTestSuite { get; set; }

        /// <summary>
        /// Flag identifying if RootTestSuite is a BOOST_DATA_TEST_CASE
        /// </summary>
        public bool DataTestCase { get; set; }
        
        #region ITestVisitor

        public void Visit(TestSuite testSuite)
        {
            Code.Require(testSuite, "testSuite");

            // A BOOST_DATA_TEST_CASE should only have child test cases
            // Any child test suite implies that the suite is not a BOOST_DATA_TEST_CASE
            DataTestCase = ((testSuite == RootTestSuite) && (testSuite.Source != null));
            
            if (DataTestCase)
            {   
                foreach (var child in testSuite.Children)
                {
                    child.Apply(this);

                    if (!DataTestCase)
                    {
                        break;
                    }
                }
            }
        }

        public void Visit(TestCase testCase)
        {
            Code.Require(testCase, "testCase");

            // A BOOST_DATA_TEST_CASE test has the following properties:
            // - It's name is of the format '_[number]' (where [number] increments per test case)
            // - It has the same line number as all other data test case instance
            // - It has the same source file reference as all other data test case instances
            DataTestCase = (
                (testCase.Source.LineNumber == RootTestSuite.Source.LineNumber) &&
                (testCase.Source.File == RootTestSuite.Source.File) &&
                _dataTestCaseNamePattern.IsMatch(testCase.Name)
            );
        }

        #endregion

        /// <summary>
        /// Determines if the provided test suite is, in fact, a BOOST_DATA_TEST_CASE
        /// </summary>
        /// <param name="testSuite">The test suite which is to be determined if it is a BOOST_DATA_TEST_CASE</param>
        /// <returns>true if the provided test suite is a BOOST_DATA_TEST_CASE; false otherwise</returns>
        public static bool IsBoostDataTestCase(TestSuite testSuite)
        {
            var verifier = new BoostDataTestCaseVerifier(testSuite);
            testSuite.Apply(verifier);
            return verifier.DataTestCase;
        }
    }
}
