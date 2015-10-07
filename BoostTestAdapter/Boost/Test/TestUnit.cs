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
        /// Identifies the fully qualified name of this TestUnit
        /// </summary>
        public string FullyQualifiedName
        {
            get
            {
                return new QualifiedNameBuilder(this).ToString();
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

        #region IXmlSerializable Helpers

        /// <summary>
        /// Xml Tag/Attribute Constants
        /// </summary>
        private static class Xml
        {
            public const string Id = "id";
            public const string Name = "name";
        }

        /// <summary>
        /// Reads common Xml attributes from a TestUnit Xml node.
        /// </summary>
        /// <param name="reader">XmlReader</param>
        protected void ReadXmlAttributes(XmlReader reader)
        {
            Utility.Code.Require(reader, "reader");

            this.Name = reader.GetAttribute(Xml.Name);

            string id = reader.GetAttribute(Xml.Id);
            if (!string.IsNullOrEmpty(id))
            {
                this.Id = int.Parse(id, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Writes common Xml attributes from a TestUnit Xml node.
        /// </summary>
        /// <param name="writer">XmlWriter</param>
        protected void WriteXmlAttributes(XmlWriter writer)
        {
            Utility.Code.Require(writer, "writer");

            if (this.Id.HasValue)
            {
                writer.WriteAttributeString(Xml.Id, this.Id.Value.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteAttributeString(Xml.Name, this.Name);
        }

        #endregion IXmlSerializable Helpers

        #region ITestVisitable

        public abstract void Apply(ITestVisitor visitor);

        #endregion ITestVisitable

        #region object overrides

        public override string ToString()
        {
            return this.FullyQualifiedName;
        }

        #endregion object overrides
    }
}