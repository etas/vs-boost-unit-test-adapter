// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
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
        
        public CommandLine ExecutionCommandLine { get; set; }

        #endregion Properties

        #region IXmlSerializable

        private static class Xml
        {
            public const string ExternalTestRunner = "ExternalTestRunner";
            public const string Type = "type";
            
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
            
            this.ExecutionCommandLine = CommandLine.FromString(reader.ReadElementString(Xml.ExecutionCommandLine));

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            Utility.Code.Require(writer, "writer");
            
            writer.WriteAttributeString(Xml.Type, this.ExtensionType.ToString());
            
            writer.WriteElementString(Xml.ExecutionCommandLine, this.ExecutionCommandLine.ToString());
        }

        #endregion IXmlSerializable

    }
}
