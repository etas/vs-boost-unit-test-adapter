// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using BoostTestAdapter.Settings;

namespace BoostTestAdapter
{
    /// <summary>
    /// Default implementation for IBoostTestDiscovererFactory.
    /// </summary>
    public class DefaultBoostTestDiscovererFactory : IBoostTestDiscovererFactory
    {
        #region IBoostTestDiscovererFactory

        /// <summary>
        /// Provides a test discoverer based on the extension type of the identifier.
        /// </summary>
        /// <param name="identifier">The output path and name of the target name along with its extension</param>
        /// <param name="options">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>An IBoostTestDiscoverer instance or null if one cannot be provided.</returns>
        public IBoostTestDiscoverer GetTestDiscoverer(string identifier, BoostTestDiscovererFactoryOptions options)
        {
            IBoostTestDiscoverer discoverer = null;

            // Prefer external test discoverers over internal ones
            if ((options != null) && (options.ExternalTestRunnerSettings != null))
            {
                discoverer = GetExternalTestDiscoverer(identifier, options.ExternalTestRunnerSettings);
            }

            if (discoverer == null)
            {
                discoverer = GetInternalTestDiscoverer(identifier);
            }

            return discoverer;
        }

        private static IBoostTestDiscoverer GetInternalTestDiscoverer(string source)
        {
            switch (Path.GetExtension(source))
            {
                case ".exe": return new BoostTestExeDiscoverer();
            }

            return null;
        }

        private static IBoostTestDiscoverer GetExternalTestDiscoverer(string source, ExternalBoostTestRunnerSettings settings)
        {
            Utility.Code.Require(settings, "settings");

            if (settings.ExtensionType == Path.GetExtension(source))
            {
                return new ExternalBoostTestDiscoverer(settings);
            }

            return null;
        }

        #endregion IBoostTestDiscovererFactory
    }
}