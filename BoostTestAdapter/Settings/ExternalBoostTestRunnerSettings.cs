// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Settings
{
    [Serializable]
    public enum DiscoveryMethodType
    {
        DiscoveryListContent,           // Use the list_content or source code internal parsing mechanisms
        DiscoveryCommandLine,           // Execute the designated command line and extract information from a TestFramework XML representation
        DiscoveryFileMap,               // Discover tests from a static list of TestFramework XML representation files

        Default = DiscoveryListContent
    }

    /// <summary>
    /// Identifies the external test runner configuration block and its configuration options.
    /// </summary>
    [XmlRoot(Xml.ExternalTestRunner)]
    public class ExternalBoostTestRunnerSettings : IXmlSerializable
    {
        #region Constants

        private static Regex _defaultExtensionType = new Regex(".dll");

        public static Regex DefaultExtensionType
        {
            get { return _defaultExtensionType; }
        }

        #endregion Constants

        #region Constructors

        public ExternalBoostTestRunnerSettings()
        {
            this.ExtensionType = DefaultExtensionType;

            this.DiscoveryMethodType = DiscoveryMethodType.Default;

            this.DiscoveryFileMap = new Dictionary<string, string>();

            this.DiscoveryCommandLine = new CommandLine();
            this.ExecutionCommandLine = new CommandLine();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Specifies the file extension pattern for which this test runner applies to
        /// </summary>
        public Regex ExtensionType { get; set; }

        /// <summary>
        /// Specifies the discovery method i.e. either via a file map or via an external command
        /// </summary>
        public DiscoveryMethodType DiscoveryMethodType { get; set; }

        /// <summary>
        /// Maps a source to a test discover Xml file path
        /// </summary>
        public IDictionary<string, string> DiscoveryFileMap { get; private set; }

        public CommandLine DiscoveryCommandLine { get; set; }

        public CommandLine ExecutionCommandLine { get; set; }

        #endregion Properties

        #region IXmlSerializable

        private static class Xml
        {
            public const string ExternalTestRunner = "ExternalTestRunner";
            public const string Type = "type";

            public const string DiscoveryCommandLine = "DiscoveryCommandLine";

            public const string DiscoveryFileMap = "DiscoveryFileMap";
            public const string DiscoveryFileMapEntry = "File";
            public const string DiscoveryFileMapSource = "source";

            public const string ExecutionCommandLine = "ExecutionCommandLine";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Utility.Code.Require(reader, "reader");

            string extension = reader.GetAttribute(Xml.Type);
            if (!string.IsNullOrEmpty(extension))
            {
                this.ExtensionType = new Regex(extension);
            }

            reader.ReadStartElement();

            reader.ConsumeUntilFirst(XmlReaderHelper.ElementFilter);

            string name = reader.Name;
            if ((name == Xml.DiscoveryCommandLine) || (name == Xml.DiscoveryFileMap))
            {
                this.DiscoveryMethodType = (DiscoveryMethodType)Enum.Parse(typeof(DiscoveryMethodType), name);

                bool empty = reader.IsEmptyElement;

                reader.ReadStartElement();
                if (name == Xml.DiscoveryCommandLine)
                {
                    empty = false;
                    this.DiscoveryCommandLine = CommandLine.FromString(reader.ReadString());
                }
                else if (name == Xml.DiscoveryFileMap)
                {
                    reader.ConsumeUntilFirst(XmlReaderHelper.ElementFilter);
                    while (reader.NodeType == XmlNodeType.Element)
                    {
                        string key = reader.GetAttribute(Xml.DiscoveryFileMapSource);

                        reader.MoveToElement();
                        empty = reader.IsEmptyElement;
                        reader.ReadStartElement();

                        this.DiscoveryFileMap[key] = (empty) ? string.Empty : reader.ReadString();

                        if (!empty)
                        {
                            reader.ReadEndElement();
                        }

                        reader.ConsumeUntilFirst(XmlReaderHelper.ElementFilter);
                    }
                }

                if (!empty)
                {
                    reader.ReadEndElement();
                }

                reader.ConsumeUntilFirst(XmlReaderHelper.ElementFilter);
            }
            
            this.ExecutionCommandLine = CommandLine.FromString(reader.ReadElementString(Xml.ExecutionCommandLine));

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            Utility.Code.Require(writer, "writer");
            
            writer.WriteAttributeString(Xml.Type, this.ExtensionType.ToString());

            if (DiscoveryMethodType == DiscoveryMethodType.DiscoveryCommandLine)
            {
                writer.WriteElementString(Xml.DiscoveryCommandLine, this.DiscoveryCommandLine.ToString());
            }
            else if (DiscoveryMethodType == DiscoveryMethodType.DiscoveryFileMap)
            {
                if (this.DiscoveryFileMap.Count > 0)
                {
                    writer.WriteStartElement(Xml.DiscoveryFileMap);

                    foreach (KeyValuePair<string, string> entry in this.DiscoveryFileMap)
                    {
                        writer.WriteStartElement(Xml.DiscoveryFileMapEntry);

                        writer.WriteAttributeString(Xml.DiscoveryFileMapSource, entry.Key);
                        writer.WriteString(entry.Value);

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }

            writer.WriteElementString(Xml.ExecutionCommandLine, this.ExecutionCommandLine.ToString());
        }

        #endregion IXmlSerializable

    }
}
