// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Utility.VisualStudio;
using BoostTestAdapter.Utility.ExecutionContext;

namespace BoostTestAdapter.Discoverers
{
    /// <summary>
    /// A Boost Test Discoverer which discovers tests based on configuration.
    /// </summary>
    internal class ExternalDiscoverer : IBoostTestDiscoverer
    {
        #region Constants

        private const string ListFileSuffix = ".test.list.xml";

        #endregion Constants

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Settings for this instance of the discoverer.</param>
        public ExternalDiscoverer(ExternalBoostTestRunnerSettings settings)
            : this(settings, new DefaultVisualStudioInstanceProvider())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Settings for this instance of the discoverer.</param>
        /// <param name="provider">Visual Studio Instance provider</param>
        public ExternalDiscoverer(ExternalBoostTestRunnerSettings settings, IVisualStudioInstanceProvider provider)
        {
            Settings = settings;
            VSProvider = provider;
        }

        /// <summary>
        /// Settings for this instance of the discoverer.
        /// </summary>
        public ExternalBoostTestRunnerSettings Settings { get; private set; }

        /// <summary>
        /// Visual Studio Instance Provider
        /// </summary>
        public IVisualStudioInstanceProvider VSProvider { get; private set; }
        
        #region IBoostTestDiscoverer

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, ITestCaseDiscoverySink discoverySink)
        {
            Code.Require(sources, "sources");
            Code.Require(discoverySink, "discoverySink");

            if (this.Settings.DiscoveryMethodType == DiscoveryMethodType.DiscoveryListContent)
            {
                // Delegate to ListContentDiscoverer
                ListContentDiscoverer discoverer = new ListContentDiscoverer(new ExternalBoostTestRunnerFactory(), VSProvider);
                discoverer.DiscoverTests(sources, discoveryContext, discoverySink);
            }            
        }

        #endregion IBoostTestDiscoverer


        /// <summary>
        /// Internal IBoostTestRunnerFactory implementation which
        /// exclusively produces ExternalBoostTestRunner instances.
        /// </summary>
        private class ExternalBoostTestRunnerFactory : IBoostTestRunnerFactory
        {
            #region IBoostTestRunnerFactory

            public IBoostTestRunner GetRunner(string source, BoostTestRunnerFactoryOptions options)
            {
                Code.Require(source, "source");
                Code.Require(options, "options");
                Code.Require(options.ExternalTestRunnerSettings, "options.ExternalTestRunnerSettings");
                
                return new ExternalBoostTestRunner(source, options.ExternalTestRunnerSettings);
            }

            #endregion IBoostTestRunnerFactory
        }
    }
}
