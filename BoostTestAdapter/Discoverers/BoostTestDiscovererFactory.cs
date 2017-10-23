// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using BoostTestAdapter.Discoverers;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility.VisualStudio;

namespace BoostTestAdapter
{
    class BoostTestDiscovererFactory : IBoostTestDiscovererFactory
    {
        #region Constructors

        /// <summary>
        /// Default constructor. The default implementation of IBoostTestRunnerFactory is provided.
        /// </summary>
        public BoostTestDiscovererFactory()
            : this(new DefaultBoostTestRunnerFactory(), new DefaultVisualStudioInstanceProvider())
        {
            // Assume that runners are to be cached for this discovery session
            TestRunnerCaching = true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">A custom IBoostTestRunnerFactory implementation.</param>
        public BoostTestDiscovererFactory(IBoostTestRunnerFactory factory, IVisualStudioInstanceProvider provider)
        {
            _factory = factory;
            _vsInstanceProvider = provider;
        }

        #endregion

        #region Members

        /// <summary>
        /// Template Boost.Test Runner factory
        /// </summary>
        /// <remarks>This instance is to be treated as a template instance by which new instances are generated from.</remarks>
        /// <seealso cref="GetTestRunnerFactory"/>
        private readonly IBoostTestRunnerFactory _factory;

        /// <summary>
        /// Visual Studio instance provider
        /// </summary>
        private readonly IVisualStudioInstanceProvider _vsInstanceProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Flag which enables/disables Boost.Test Runner caching for this discovery session
        /// </summary>
        public bool TestRunnerCaching { get; set; } = false;

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

            var factory = GetTestRunnerFactory();

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
                if (string.Compare(extension, BoostTestDiscoverer.ExeExtension, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }
                
                // Ensure that the source is a Boost.Test module if it supports '--list_content'
                if (IsListContentSupported(factory, source, settings))
                {
                    listContentDiscovererSources.Add(source);
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
                    Discoverer = new ListContentDiscoverer(factory, _vsInstanceProvider),
                    Sources = listContentDiscovererSources
                });
     
            return discoverers;
        }

        #endregion

        /// <summary>
        /// Provides a Boost.Test runner factory for this discovery 'session'
        /// </summary>
        /// <returns>A Boost.Test runner factory or null if one cannot be generated</returns>
        private IBoostTestRunnerFactory GetTestRunnerFactory()
        {
            if ((_factory != null) && TestRunnerCaching)
            {
                return new CachingBoostTestRunnerFactory(_factory);
            }

            return _factory;
        }

        /// <summary>
        /// Determines whether the provided source has --list_content capabilities
        /// </summary>
        /// <param name="source">The source to test</param>
        /// <param name="settings">Test adapter settings</param>
        /// <returns>true if the source has list content capabilities; false otherwise</returns>
        private static bool IsListContentSupported(IBoostTestRunnerFactory factory, string source, BoostTestAdapterSettings settings)
        {
            var runner = factory.GetRunner(source, settings.TestRunnerFactoryOptions);

            return (runner != null) && (runner.Capabilities.ListContent);
        }
    }
}
