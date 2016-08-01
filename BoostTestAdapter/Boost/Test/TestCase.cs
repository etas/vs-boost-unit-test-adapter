// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Test
{
    public class TestCase : TestUnit
    {
        #region Constructors

        /// <summary>
        /// Constructor. Required as per IXmlSerializable requirements.
        /// </summary>
        public TestCase() :
            this(null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Test Unit (local) name.</param>
        /// <param name="parent">Parent/Owner Test Unit of this instance.</param>
        public TestCase(string name, TestSuite parent)
            : base(name, parent)
        {
        }

        #endregion Constructors

        #region ITestVisitable

        public override void Apply(ITestVisitor visitor)
        {
            Utility.Code.Require(visitor, "visitor");

            visitor.Visit(this);
        }

        #endregion ITestVisitable
    }
}