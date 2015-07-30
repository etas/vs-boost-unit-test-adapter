using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BoostTestAdapter.Boost.Test
{
    [XmlRoot(Xml.BoostTestFramework)]
    public class TestFramework : IXmlSerializable
    {
        #region Constructors

        public TestFramework() :
            this(null, null)
        {
        }

        public TestFramework(string source, TestSuite master)
        {
            this.Source = source;
            this.MasterTestSuite = master;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Fully qualified path detailing the source Dll/EXE which contains these tests
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Boost Test Master Test Suite
        /// </summary>
        public TestSuite MasterTestSuite { get; private set; }

        #endregion Properties

        #region IXmlSerializable

        /// <summary>
        /// Xml Tag/Attribute Constants
        /// </summary>
        private static class Xml
        {
            public const string BoostTestFramework = "BoostTestFramework";
            public const string Source = "source";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Utility.Code.Require(reader, "reader");

            reader.MoveToElement();

            this.Source = reader.GetAttribute(Xml.Source);

            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement(Xml.BoostTestFramework);

            if (!empty)
            {
                XmlSerializer deserialiser = new XmlSerializer(typeof(TestSuite));
                this.MasterTestSuite = deserialiser.Deserialize(reader) as TestSuite;

                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            Utility.Code.Require(writer, "writer");

            writer.WriteAttributeString(Xml.Source, this.Source);

            if (this.MasterTestSuite != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TestSuite));
                serializer.Serialize(writer, this.MasterTestSuite);
            }
        }

        #endregion IXmlSerializable
    }
}