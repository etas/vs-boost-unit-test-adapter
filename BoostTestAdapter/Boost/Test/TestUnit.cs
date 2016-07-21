// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Test
{
    /// <summary>
    /// Base class for Boost Test test components. Follows the composite design pattern.
    /// </summary>
    public abstract class TestUnit : ITestVisitable
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Test Unit (local) name.</param>
        /// <param name="parent">Parent/Owner Test Unit of this instance.</param>
        protected TestUnit(string name, TestUnit parent)
        {
            this.Id = null;
            this.Name = name;
            this.Parent = parent;
            this.Labels = Enumerable.Empty<string>();
            
            if (parent != null)
            {
                parent.AddChild(this);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Test Unit Id. Optional.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Test Unit (local) Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Parent/Owner Test Unit of this instance.
        /// </summary>
        public TestUnit Parent { get; private set; }

        /// <summary>
        /// Child Test Units of this instance.
        /// </summary>
        public virtual IEnumerable<TestUnit> Children
        {
            get
            {
                return Enumerable.Empty<TestUnit>();
            }
        }

        /// <summary>
        /// Optional source file information related to this test unit.
        /// </summary>
        public SourceFileInfo Source { get; set; }

        /// <summary>
        /// Identifies any label associations with this test unit
        /// </summary>
        public IEnumerable<string> Labels { get; set; }

        /// <summary>
        /// Cached version of the fully qualified name builder
        /// </summary>
        private QualifiedNameBuilder _fullyQualifiedName = null;

        /// <summary>
        /// Internal property which provides a cached QualifiedNameBuilder to represent the fully qualified name
        /// </summary>
        private QualifiedNameBuilder FullyQualifiedNameBuilder
        {
            get
            {
                if (this._fullyQualifiedName == null)
                {
                    _fullyQualifiedName = (Parent == null) ? new QualifiedNameBuilder() : Parent.FullyQualifiedNameBuilder.Clone();
                    _fullyQualifiedName.Push(this);
                }

                return _fullyQualifiedName;
            }
        }
        
        /// <summary>
        /// Identifies the fully qualified name of this TestUnit
        /// </summary>
        public string FullyQualifiedName
        {
            get
            {
                return FullyQualifiedNameBuilder.ToString();
            }
        }

        #endregion Properties

        /// <summary>
        /// Adds a child to this TestUnit
        /// </summary>
        /// <param name="unit">The unit to add as a child</param>
        public virtual void AddChild(TestUnit unit)
        {
            throw new InvalidOperationException();
        }
        
        #region ITestVisitable

        public abstract void Apply(ITestVisitor visitor);

        #endregion ITestVisitable

        #region Utility

        /// <summary>
        /// Given a fully qualified name of a <b>test case</b>, generates the respective test unit hierarchy.
        /// </summary>
        /// <param name="fullyQualifiedName">The fully qualified name of the <b>test case</b></param>
        /// <returns>The test case hierarcy represented by the provided fully qualified name</returns>
        public static TestCase FromFullyQualifiedName(string fullyQualifiedName)
        {
            return FromFullyQualifiedName(QualifiedNameBuilder.FromString(fullyQualifiedName));
        }
        
        /// <summary>
        /// Given a fully qualified name of a <b>test case</b>, generates the respective test unit hierarchy.
        /// </summary>
        /// <param name="fullyQualifiedName">The fully qualified name of the <b>test case</b></param>
        /// <returns>The test case hierarchy represented by the provided fully qualified name</returns>
        /// <remarks>The parameter 'fullyQualifiedName' will be modified and emptied in due process</remarks>
        private static TestCase FromFullyQualifiedName(QualifiedNameBuilder fullyQualifiedName)
        {
            // Reverse the fully qualified name stack i.e. Master Test Suite should be first element and Test Case should be last element
            Stack<string> hierarchy = new Stack<string>();
            while (fullyQualifiedName.Peek() != null)
            {
                hierarchy.Push(fullyQualifiedName.Peek());
                fullyQualifiedName.Pop();
            }

            TestSuite parent = null;

            // Treat each entry (except for the last) as a test suite
            while (hierarchy.Count > 1)
            {
                parent = new TestSuite(hierarchy.Peek(), parent);
                hierarchy.Pop();
            }

            // Treat the last entry as a test case
            return (hierarchy.Count == 1) ? new TestCase(hierarchy.Peek(), parent) : null;
        }

        #endregion Utility

        #region object overrides

        public override string ToString()
        {
            return this.FullyQualifiedName;
        }

        #endregion object overrides
    }
}