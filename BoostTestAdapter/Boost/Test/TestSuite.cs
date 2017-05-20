// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Test
{
    public class TestSuite : TestUnit
    {
        #region Members

        private List<TestUnit> _children = null;

        #endregion Members

        #region Constructors

        /// <summary>
        /// Constructor. Required as per IXmlSerializable requirements.
        /// </summary>
        public TestSuite() :
            this(null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Test Unit (local) name.</param>
        public TestSuite(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Test Unit (local) name.</param>
        /// <param name="parent">Parent/Owner Test Unit of this instance.</param>
        public TestSuite(string name, TestSuite parent)
            : base(name, parent)
        {
            this._children = new List<TestUnit>();
        }

        #endregion Constructors

        #region Properties

        public override IEnumerable<TestUnit> Children
        {
            get
            {
                return this._children;
            }
        }

        #endregion Properties

        public override void AddChild(TestUnit unit)
        {
            this._children.Add(unit);
        }

 
        #region ITestVisitable

        public override void Apply(ITestVisitor visitor)
        {
            Utility.Code.Require(visitor, "visitor");

            visitor.Visit(this);
        }

        #endregion ITestVisitable
    }
}