// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace BoostTestAdapterNunit.Utility.Xml
{
    /// <summary>
    /// Interface which identifies a mechanism for filtering particular xml nodes.
    /// </summary>
    public interface IXmlNodeFilter
    {
        /// <summary>
        /// States whether the node should be filtered or not
        /// </summary>
        /// <param name="node">The node to test</param>
        /// <returns>true if the node should be filtered; false otherwise</returns>
        bool Filter(XmlNode node);
    }
    
    #region Default IXMLNodeFilter Implementations

    /// <summary>
    /// IXmlNodeFilter implementation. Does not filter any xml node.
    /// </summary>
    public class NullFilter : IXmlNodeFilter
    {
        public bool Filter(XmlNode node)
        {
            return false;
        }
    }

    /// <summary>
    /// IXmlNodeFilter implementation. Filter each and every xml node.
    /// </summary>
    public class AllFilter : IXmlNodeFilter
    {
        public bool Filter(XmlNode node)
        {
            return true;
        }
    }

    #endregion 
    
    /// <summary>
    /// IXmlNodeFilter implementation. Filters xml nodes based on their NodeType.
    /// </summary>
    public class XmlNodeTypeFilter : IXmlNodeFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filtered">The collection of XmlNodeTypes which should be filtered out</param>
        public XmlNodeTypeFilter(IEnumerable<XmlNodeType> filtered)
        {
            this.FilteredTypes = filtered;
        }

        /// <summary>
        /// A collection of XmlNodeTypes which are to be filtered out
        /// </summary>
        public IEnumerable<XmlNodeType> FilteredTypes { get; private set; }

        #region IXmlNodeFilter

        /// <summary>
        /// States whether the node should be filtered or not
        /// </summary>
        /// <param name="node">The node to test</param>
        /// <returns>true if the node should be filtered; false otherwise</returns>
        public bool Filter(XmlNode node)
        {
            return Filter(node.NodeType);
        }

        #endregion IXmlNodeFilter

        /// <summary>
        /// States whether the node type should be filtered or not
        /// </summary>
        /// <param name="type">The node type to test</param>
        /// <returns>true if the node type should be filtered; false otherwise</returns>
        public bool Filter(XmlNodeType type)
        {
            return this.FilteredTypes.Contains(type);
        }
        
        /// <summary>
        /// Creates an XmlNodeTypeFilter which does not filter out anything
        /// </summary>
        public static XmlNodeTypeFilter None
        {
            get
            {
                return new XmlNodeTypeFilter(Enumerable.Empty<XmlNodeType>());
            }
        }

        /// <summary>
        /// Creates a default XmlNodeTypeFilter used within most of the tests
        /// </summary>
        public static XmlNodeTypeFilter DefaultFilter
        {
            get
            {
                return new XmlNodeTypeFilter(new XmlNodeType[] { XmlNodeType.CDATA, XmlNodeType.Comment, XmlNodeType.ProcessingInstruction, XmlNodeType.XmlDeclaration });
            }
        }

    }
    
    public class XmlComparer
    {

        private IEnumerator GetXmlCollectionEnumerator(IEnumerable enumerable)
        {
            return (enumerable == null) ? null : enumerable.GetEnumerator();
        }

        /// <summary>
        /// Loads an Xml string fragment
        /// </summary>
        /// <param name="xml">the Xml string fragment</param>
        /// <returns>The Xml DOM representation</returns>
        private XmlDocument LoadXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc;
        }

        /// <summary>
        /// Compares the 2 Xml collections.
        /// </summary>
        /// <param name="lhs">The left-hand side Xml collection enumerator</param>
        /// <param name="rhs">The right-hand side Xml collection enumerator</param>
        /// <param name="filter">The XmlNodeType filter to apply during comparison</param>
        private void CompareXML(IEnumerator lhs, IEnumerator rhs, IXmlNodeFilter filter)
        {
            bool lhsNext = lhs != null && lhs.MoveNext();
            bool rhsNext = rhs != null && rhs.MoveNext();

            while (lhsNext && rhsNext)
            {
                XmlNode lhsChild = (XmlNode)lhs.Current;
                XmlNode rhsChild = (XmlNode)rhs.Current;

                if (filter.Filter(lhsChild))
                {
                    lhsNext = lhs.MoveNext();
                    continue;
                }
                else if (filter.Filter(rhsChild))
                {
                    rhsNext = rhs.MoveNext();
                    continue;
                }

                _CompareXML(lhsChild, rhsChild, filter);

                lhsNext = lhs.MoveNext();
                rhsNext = rhs.MoveNext();
            }

            // Ensure that any remaining nodes are filtered
            // and not important for the comparison process
            while (lhsNext)
            {
                Assert.IsTrue(filter.Filter((XmlNode)lhs.Current));
                lhsNext = lhs.MoveNext();
            }

            while (rhsNext)
            {
                Assert.IsTrue(filter.Filter((XmlNode)rhs.Current));
                rhsNext = rhs.MoveNext();
            }
        }

        /// <summary>
        /// Internal version of CompareXML. Compares the 2 Xml subtrees.
        /// </summary>
        /// <remarks>In contrast to the public CompareXML, this version avoids checking whether both elements should be filtered or not.</remarks>
        /// <param name="lhs">The left-hand side Xml subtree</param>
        /// <param name="rhs">The right-hand side Xml subtree</param>
        /// <param name="filter">The XmlNodeType filter to apply during comparison</param>
        private void _CompareXML(XmlNode lhs, XmlNode rhs, IXmlNodeFilter filter)
        {
            Assert.AreEqual(lhs.NodeType, rhs.NodeType);

            Assert.AreEqual(lhs.NamespaceURI, rhs.NamespaceURI);
            Assert.AreEqual(lhs.LocalName, rhs.LocalName);

            if (lhs.NodeType == XmlNodeType.CDATA || lhs.NodeType == XmlNodeType.Comment || lhs.NodeType == XmlNodeType.Text)
            {
                Assert.AreEqual(lhs.Value, rhs.Value);
            }

            CompareXML(GetXmlCollectionEnumerator(lhs.ChildNodes), GetXmlCollectionEnumerator(rhs.ChildNodes), filter);
            CompareXML(GetXmlCollectionEnumerator(lhs.Attributes), GetXmlCollectionEnumerator(rhs.Attributes), filter);
        }

        /// <summary>
        /// Compares the 2 Xml subtrees.
        /// </summary>
        /// <param name="lhs">The left-hand side Xml subtree</param>
        /// <param name="rhs">The right-hand side Xml subtree</param>
        /// <param name="filter">The XmlNodeType filter to apply during comparison</param>
        public void CompareXML(XmlNode lhs, XmlNode rhs, IXmlNodeFilter filter)
        {
            bool lhsFilter = filter.Filter(lhs);
            bool rhsFilter = filter.Filter(rhs);

            Assert.AreEqual(lhsFilter, rhsFilter);

            // At this point, lhsFilter and rhsFilter are known to be equal.
            // If both elements should be filtered, avoid comparing the subtrees altogether.
            if (!lhsFilter)
            {
                _CompareXML(lhs, rhs, filter);
            }
        }

        /// <summary>
        /// Compares the 2 Xml string fragments.
        /// </summary>
        /// <param name="lhs">The left-hand side Xml fragment</param>
        /// <param name="rhs">The right-hand side Xml fragment</param>
        /// <param name="filter">The XmlNodeType filter to apply during comparison</param>
        public void CompareXML(string lhs, string rhs, IXmlNodeFilter filter)
        {
            this.CompareXML(LoadXml(lhs), LoadXml(rhs), filter);
        }
    }
}
