// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter.Discoverers;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using System;

namespace BoostTestAdapter
{
    class BoostTestDiscovererFactory : IBoostTestDiscovererFactory
    {
        #region Constants

        private static string ForceListContentExtension { get { return ".test.boostd.exe"; } }

        #endregion 

        #region Constructors

        /// <summary>
        /// Default constructor. The default implementation of IBoostTestRunnerFactory is provided.
        /// </summary>
        public BoostTestDiscovererFactory()
            : this(new DefaultBoostTestRunnerFactory())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">A custom IBoostTestRunnerFactory implementation.</param>
        public BoostTestDiscovererFactory(IBoostTestRunnerFactory factory)
        {
            _factory = factory;
        }

        #endregion

        #region Members

        private readonly IBoostTestRunnerFactory _factory;

        #endregion

        #region IBoostTestDiscovererFactory

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
            settings = settings ?? new BoostTestAdapterSettings();

            // sources that can be run on the external runner
            var externalDiscovererSources = new List<string>();

            // sources that support list-content parameter
            var listContentDiscovererSources = new List<string>();

            // sources that do NOT support the list-content parameter
            var sourceCodeDiscovererSources = new List<string>();
            
            foreach (var source in sources)
            {
                string extension = Path.GetExtension(source);

                if (settings.ExternalTestRunner != null)
                {
                    if (settings.ExternalTestRunner.ExtensionType.IsMatch(extension))
                    {
                        externalDiscovererSources.Add(source);
                        continue;
                    }
                }

                // Skip modules which are not .exe
                if (extension != BoostTestDiscoverer.ExeExtension)
                    continue;

                if (((settings.ForceListContent) || IsListContentSupported(source, settings)))
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
     
            return discoverers;
        }

        #endregion

        /// <summary>
        /// Determines whether the provided source has --list_content capabilities
        /// </summary>
        /// <param name="source">The source to test</param>
        /// <param name="settings">Test adapter settings</param>
        /// <returns>true if the source has list content capabilities; false otherwise</returns>
        private bool IsListContentSupported(string source, BoostTestAdapterSettings settings)
        {
            BoostTestRunnerFactoryOptions options = new BoostTestRunnerFactoryOptions()
            {
                ExternalTestRunnerSettings = settings.ExternalTestRunner
            };

            IBoostTestRunner runner = _factory.GetRunner(source, options);

            // Convention over configuration. Assume test runners utilising such an extension
            return (runner != null) && (runner.Source.EndsWith(ForceListContentExtension, StringComparison.OrdinalIgnoreCase) || runner.ListContentSupported);
        }

    }
}
