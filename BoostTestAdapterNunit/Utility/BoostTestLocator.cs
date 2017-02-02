// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Boost.Test;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// ITestVisitor implementation which looks up test units based on their qualified name.
    /// </summary>
    public class BoostTestLocator : ITestVisitor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fullyQualifiedName">The test unit's fully qualified name to locate</param>
        private BoostTestLocator(string fullyQualifiedName)
        {
            this.FullyQualifiedName = fullyQualifiedName;
        }

        /// <summary>
        /// The fully qualified name of the test unit to locate
        /// </summary>
        public string FullyQualifiedName { get; private set; }

        /// <summary>
        /// The resultant test unit
        /// </summary>
        public TestUnit Unit { get; private set; }

        #region ITestVisitor

        public void Visit(TestCase testCase)
        {
            Check(testCase);
        }

        public void Visit(TestSuite testSuite)
        {
            if (!Check(testSuite))
            {
                foreach (TestUnit child in testSuite.Children)
                {
                    child.Apply(this);

                    if (this.Unit != null)
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Checks if the provided test unit is the one which was requested for location
        /// </summary>
        /// <param name="unit">The test unit to test</param>
        /// <returns>true if the provided test unit was the one requested for location; false otherwise</returns>
        private bool Check(TestUnit unit)
        {
            bool match = (unit.FullyQualifiedName == this.FullyQualifiedName);

            if (match)
            {
                this.Unit = unit;
            }

            return match;
        }

        /// <summary>
        /// Locates the test unit with the specified fully qualified name from the provided test framework
        /// </summary>
        /// <param name="root">The framework from which to locate the test unit</param>
        /// <param name="fullyQualifiedName">The test unit's fully qualified name to locate</param>
        /// <returns>The located test unit or null if it was not found</returns>
        public static TestUnit Locate(TestFramework root, string fullyQualifiedName)
        {
            return Locate(root?.MasterTestSuite, fullyQualifiedName);
        }

        /// <summary>
        /// Locates the test unit with the specified fully qualified name from the provided root
        /// </summary>
        /// <param name="root">The root test unit from which to locate the test unit</param>
        /// <param name="fullyQualifiedName">The test unit's fully qualified name to locate</param>
        /// <returns>The located test unit or null if it was not found</returns>
        public static TestUnit Locate(TestUnit root, string fullyQualifiedName)
        {
            if (root == null)
            {
                return null;
            }

            var locator = new BoostTestLocator(fullyQualifiedName);
            root.Apply(locator);
            return locator.Unit;
        }
    }
}
