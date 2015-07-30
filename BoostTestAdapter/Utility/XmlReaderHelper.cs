using System;
using System.Xml;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Helper functionality related to System.Xml.XmlReader
    /// </summary>
    internal static class XmlReaderHelper
    {
        /// <summary>
        /// Default filter for Xml Elements
        /// </summary>
        public static readonly XmlNodeType[] ElementFilter = new XmlNodeType[] { XmlNodeType.Element, XmlNodeType.EndElement };

        /// <summary>
        /// Consumes nodes from the reader until the first ocurance of the XmlNodeType identified within types.
        /// </summary>
        /// <param name="reader">The reader from which to consume Xml nodes</param>
        /// <param name="types">The XmlNodeType types of interest which will halt consumption</param>
        public static void ConsumeUntilFirst(this XmlReader reader, XmlNodeType[] types)
        {
            while (Array.IndexOf(types, reader.NodeType) < 0)
            {
                reader.Read();
            }
        }
    }
}