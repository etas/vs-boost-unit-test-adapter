﻿// (C) Copyright ETAS 2015.
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
    [XmlRoot(Xml.TestCase)]
    public class TestCase : TestUnit, IXmlSerializable
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

        #region Properties

        /// <summary>
        /// Optional source file information related to this test.
        /// </summary>
        public SourceFileInfo Source { get; set; }

        #endregion Properties

        #region IXmlSerializable

        /// <summary>
        /// Xml Tag/Attribute Constants
        /// </summary>
        internal static class Xml
        {
            public const string TestCase = "TestCase";
            public const string File = "file";
            public const string Line = "line";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            base.ReadXmlAttributes(reader);

            string file = reader.GetAttribute(Xml.File);

            if (!string.IsNullOrEmpty(file))
            {
                this.Source = new SourceFileInfo(file);
                this.Source.LineNumber = int.Parse(reader.GetAttribute(Xml.Line), CultureInfo.InvariantCulture);
            }

            reader.MoveToElement();
            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement(Xml.TestCase);

            if (!empty)
            {
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            base.WriteXmlAttributes(writer);

            if (this.Source != null)
            {
                writer.WriteAttributeString(Xml.File, this.Source.File);
                writer.WriteAttributeString(Xml.Line, this.Source.LineNumber.ToString(CultureInfo.InvariantCulture));
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