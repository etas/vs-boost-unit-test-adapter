// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.ComponentModel.Composition;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace BoostTestAdapter.Settings
{
    /// <summary>
    /// A Visual Studio ISettingsProvider implementation. Provisions BoostTestAdapterSettings.
    /// </summary>
    [Export(typeof(ISettingsProvider))]
    [SettingsName(BoostTestAdapterSettings.XmlRootName)]
    public class BoostTestAdapterSettingsProvider : ISettingsProvider
    {
        #region Constructors

        public BoostTestAdapterSettingsProvider()
        {
            this.Settings = new BoostTestAdapterSettings();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Reference to the recently loaded settings. May be null if no settings were specified or the settings failed to load.
        /// </summary>
        public BoostTestAdapterSettings Settings { get; private set; }

        #endregion Properties

        #region ISettingsProvider

        public void Load(XmlReader reader)
        {
            Utility.Code.Require(reader, "reader");

            // NOTE This method gets called if the settings name matches the node name as expected.

            if (reader.Read() && reader.Name.Equals(BoostTestAdapterSettings.XmlRootName))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(BoostTestAdapterSettings));
                this.Settings = deserializer.Deserialize(reader) as BoostTestAdapterSettings;
            }
        }

        #endregion ISettingsProvider

        /// <summary>
        /// Builds a BoostTestAdapterSettings structure based on the information located within the IDiscoveryContext instance.
        /// </summary>
        /// <param name="context">The discovery context instance</param>
        /// <returns>A BoostTestRunnerSettings instance based on the information identified via the provided IDiscoveryContext instance.</returns>
        public static BoostTestAdapterSettings GetSettings(IDiscoveryContext context)
        {
            Utility.Code.Require(context, "context");

            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();

            BoostTestAdapterSettingsProvider provider = (context.RunSettings == null) ? null : context.RunSettings.GetSettings(BoostTestAdapterSettings.XmlRootName) as BoostTestAdapterSettingsProvider;
            if (provider != null)
            {
                settings = provider.Settings;
            }

            // Return defaults
            return settings;
        }
    }
}