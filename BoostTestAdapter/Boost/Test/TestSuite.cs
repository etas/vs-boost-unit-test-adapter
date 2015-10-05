// (C) Copyright ETAS 2015.
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
    [XmlRoot(Xml.TestSuite)]
    public class TestSuite : TestUnit, IXmlSerializable
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

        #region IXmlSerializable

        /// <summary>
        /// Xml Tag/Attribute Constants
        /// </summary>
        internal static class Xml
        {
            public const string TestSuite = "TestSuite";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            base.ReadXmlAttributes(reader);

            reader.MoveToElement();
            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement(Xml.TestSuite);

            if (!empty)
            {
                reader.ConsumeUntilFirst(XmlReaderHelper.ElementFilter);

                while (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == Xml.TestSuite)
                    {
                        new TestSuite(null, this).ReadXml(reader);
                    }
                    else if (reader.Name == TestCase.Xml.TestCase)
                    {
                        new TestCase(null, this).ReadXml(reader);
                    }

                    reader.ConsumeUntilFirst(XmlReaderHelper.ElementFilter);
                }

                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            base.WriteXmlAttributes(writer);

            foreach (TestUnit child in this.Children)
            {
                child.Apply(new BoostTestXmlVisitor(writer));
            }
        }

        private class BoostTestXmlVisitor : ITestVisitor
        {
            private XmlSerializer SuiteSerializer { get; set; }

            private XmlSerializer CaseSerializer { get; set; }

            private XmlWriter Writer { get; set; }

            public BoostTestXmlVisitor(XmlWriter writer)
            {
                this.SuiteSerializer = new XmlSerializer(typeof(TestSuite));
                this.CaseSerializer = new XmlSerializer(typeof(TestCase));

                this.Writer = writer;
            }

            public void Visit(TestCase testCase)
            {
                this.CaseSerializer.Serialize(this.Writer, testCase);
            }

            public void Visit(TestSuite testSuite)
            {
                this.SuiteSerializer.Serialize(this.Writer, testSuite);
            }
        }

        #endregion IXmlSerializable

        #region ITestVisitable

        public override void Apply(ITestVisitor visitor)
        {
            Utility.Code.Require(visitor, "visitor");

            visitor.Visit(this);
        }

        #endregion ITestVisitable
    }
}