// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter.Discoverers;
using System.Text.RegularExpressions;

namespace BoostTestAdapter
{
    class BoostTestDiscovererFactory : IBoostTestDiscovererFactory
    {
        #region Constructors

        /// <summary>
        /// Default constructor. The default implementation of IListContentHelper is provided.
        /// </summary>
        public BoostTestDiscovererFactory()
            : this(new ListContentHelper())
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listContentHelper">A custom IListContentHelper implementation.</param>
        public BoostTestDiscovererFactory(IListContentHelper listContentHelper)
        {
            _listContentHelper = listContentHelper;
        }

        #endregion


        #region Members

        private readonly IListContentHelper _listContentHelper;

        #endregion


        #region IBoostTestDiscovererFactory

        /// <summary>
        /// Returns an IBoostTestDiscoverer based on the provided source.
        /// </summary>
        /// <param name="source">Source to be associated.</param>
        /// <param name="settings">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>An IBoostTestDiscoverer instance or null if one cannot be provided.</returns>
        public IBoostTestDiscoverer GetDiscoverer(string source, Settings.BoostTestAdapterSettings settings)
        {
            var list = new[] { source };
            var results = GetDiscoverers(list, settings);
            if (results != null)
            {
                var result = results.FirstOrDefault(x => x.Sources.Contains(source));
                if (result != null)
                    return result.Discoverer;
            }

            return null;
        }

        /// <summary>
        /// Associates each source with the correct IBoostTestDiscoverer implementation.
        /// </summary>
        /// <param name="sources">List of the sources to be associated.</param>
        /// <param name="settings">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>A list of FactoryResult. Each value contains an instance of IBoostTestDiscoverer and a list of sources that should be analyzed by the discoverer.</returns>
        /// <remarks>The returned IEnumerable is always an already initialised List instance.</remarks>
        public IEnumerable<FactoryResult> GetDiscoverers(IReadOnlyCollection<string> sources, Settings.BoostTestAdapterSettings settings)
        {
            var discoverers = new List<FactoryResult>();

            if ((sources == null) || (!sources.Any()))
                return discoverers;

            // Use default settings in case they are not provided by client code
            settings = settings ?? new Settings.BoostTestAdapterSettings();

            // sources that can be run on the external runner
            var externalDiscovererSources = new List<string>();

            // sources that support list-content parameter
            var listContentDiscovererSources = new List<string>();

            // sources that do NOT support the list-content parameter
            var sourceCodeDiscovererSources = new List<string>();

            _listContentHelper.Timeout = settings.DiscoveryTimeoutMilliseconds;

            foreach (var source in sources)
            {
                if (settings.ExternalTestRunner != null)
                {
                    Regex matcher = new Regex(settings.ExternalTestRunner.ExtensionType);
                    if (matcher.IsMatch(source))
                    {
                        externalDiscovererSources.Add(source);
                        continue;
                    }
                }

                // Skip modules which are not .exe
                if (Path.GetExtension(source) != BoostTestDiscoverer.ExeExtension)
                    continue;

                if (settings.UseListContent && _listContentHelper.IsListContentSupported(source))
                {
                    listContentDiscovererSources.Add(source);
                }
                else
                {
                    sourceCodeDiscovererSources.Add(source);
                }
            }

            if ((externalDiscovererSources.Any()) && (settings != null))
            {
                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new ExternalDiscoverer(settings.ExternalTestRunner),
                    Sources = externalDiscovererSources
                });
            }

            if (listContentDiscovererSources.Any())
                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new ListContentDiscoverer(),
                    Sources = listContentDiscovererSources
                });

            if (sourceCodeDiscovererSources.Any())
                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new SourceCodeDiscoverer(),
                    Sources = sourceCodeDiscovererSources
                });

            return discoverers;
        }

        #endregion
    }
}
