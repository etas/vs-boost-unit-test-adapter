// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Collections.Generic;

using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Test
{
    /// <summary>
    /// Allows building TestFrameworks with ease using the Builder pattern.
    /// </summary>
    public class TestFrameworkBuilder
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Boost Test EXE/Dll file path</param>
        /// <param name="name">Name of Master Test Suite</param>
        public TestFrameworkBuilder(string source, string name) :
            this(source, name, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Boost Test EXE/Dll file path</param>
        /// <param name="name">Name of Master Test Suite</param>
        /// <param name="id">Id of Master Test Suite</param>
        public TestFrameworkBuilder(string source, string name, int? id)
        {
            this.Source = source;

            if (string.IsNullOrEmpty(name))
            {
                name = QualifiedNameBuilder.DefaultMasterTestSuiteName;
            }

            this.MasterTestSuite = new TestSuite(name, null);
            this.MasterTestSuite.Id = id;

            this.Parent = this.MasterTestSuite;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Boost Test EXE/Dll file path
        /// </summary>
        private string Source { get; set; }

        /// <summary>
        /// Master Test Suite
        /// </summary>
        private TestSuite MasterTestSuite { get; set; }

        /// <summary>
        /// Current TestSuite Parent
        /// </summary>
        private TestSuite Parent { get; set; }

        #endregion Properties

        /// <summary>
        /// Builds a new TestSuite. Starts a new context in which
        /// newly created TestUnits will be parented to this TestSuite.
        /// </summary>
        /// <param name="name">Test Suite Name</param>
        /// <returns>this</returns>
        public TestFrameworkBuilder TestSuite(string name)
        {
            return this.TestSuite(name, null);
        }

        /// <summary>
        /// Builds a new TestSuite. Starts a new context in which
        /// newly created TestUnits will be parented to this TestSuite.
        /// </summary>
        /// <param name="name">Test Suite Name</param>
        /// <param name="id">Test Suite Id</param>
        /// <returns>this</returns>
        public TestFrameworkBuilder TestSuite(string name, int? id)
        {
            return this.TestSuite(name, id, null);
        }

        /// <summary>
        /// Builds a new TestSuite. Starts a new context in which
        /// newly created TestUnits will be parented to this TestSuite.
        /// </summary>
        /// <param name="name">Test Suite Name</param>
        /// <param name="id">Test Suite Id</param>
        /// <param name="source">Test Suite source file debug information</param>
        /// <returns>this</returns>
        public TestFrameworkBuilder TestSuite(string name, int? id, SourceFileInfo source)
        {
            TestSuite testSuite = new TestSuite(name, this.Parent);
            testSuite.Id = id;
            testSuite.Source = source;

            this.Parent = testSuite;

            return this;
        }

        /// <summary>
        /// Builds a new TestCase.
        /// </summary>
        /// <param name="name">Test Case Name</param>
        /// <returns>this</returns>
        public TestFrameworkBuilder TestCase(string name)
        {
            return this.TestCase(name, null);
        }

        /// <summary>
        /// Builds a new TestCase.
        /// </summary>
        /// <param name="name">Test Case Name</param>
        /// <param name="id">Test Case Id</param>
        /// <returns>this</returns>
        public TestFrameworkBuilder TestCase(string name, int? id)
        {
            return this.TestCase(name, id, null);
        }

        /// <summary>
        /// Builds a new TestCase.
        /// </summary>
        /// <param name="name">Test Case Name</param>
        /// <param name="id">Test Case Id</param>
        /// <param name="source">Test Case source file debug information</param>
        /// <returns>this</returns>
        public TestFrameworkBuilder TestCase(string name, int? id, SourceFileInfo source)
        {
            return this.TestCase(name, id, source, Enumerable.Empty<string>());
        }

        /// <summary>
        /// Builds a new TestCase.
        /// </summary>
        /// <param name="name">Test Case Name</param>
        /// <param name="id">Test Case Id</param>
        /// <param name="source">Test Case source file debug information</param>
        /// <param name="labels">Test Case labels</param>
        /// <returns>this</returns>
        public TestFrameworkBuilder TestCase(string name, int? id, SourceFileInfo source, IEnumerable<string> labels)
        {
            TestCase testCase = new TestCase(name, this.Parent);

            testCase.Id = id;
            testCase.Source = source;
            testCase.Labels = labels;

            return this;
        }

        /// <summary>
        /// Ends the current TestSuite context and moves up one level in the hierarchy.
        /// </summary>
        /// <returns>this</returns>
        public TestFrameworkBuilder EndSuite()
        {
            this.Parent = (TestSuite)this.Parent.Parent;

            return this;
        }

        /// <summary>
        /// Builds the TestFramework.
        /// </summary>
        /// <returns>The TestFramework</returns>
        public TestFramework Build()
        {
            return new TestFramework(this.Source, this.MasterTestSuite);
        }
    }
}