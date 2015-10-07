// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter
{
    [FileExtension(DllExtension)]
    [FileExtension(ExeExtension)]
    [DefaultExecutorUri(BoostTestExecutor.ExecutorUriString)]
    public class BoostTestDiscoverer : ITestDiscoverer
    {
        #region Constants

        public const string DllExtension = ".dll";
        public const string ExeExtension = ".exe";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Default constructor (the one that is called off by Visual Studio before being able to call method DiscoverTests)
        /// </summary>
        public BoostTestDiscoverer()
            :this(new DefaultBoostTestDiscovererFactory())
        {
        }

        /// <summary>
        /// Constructor accepting an object of type IBoostTestDiscovererFactory (for mocking)
        /// </summary>
        /// <param name="newTestDiscovererFactory"></param>
        public BoostTestDiscoverer(IBoostTestDiscovererFactory newTestDiscovererFactory)
        {
            this.TestDiscovererFactory = newTestDiscovererFactory;
        }

        #endregion Constructors

        #region Properties

        private IBoostTestDiscovererFactory TestDiscovererFactory { get; set; }

        #endregion Properties

        #region ITestDiscoverer

        /// <summary>
        /// Method call by Visual studio ("discovered via reflection") for test enumeration
        /// </summary>
        /// <param name="sources">path, target name and target extensions to discover</param>
        /// <param name="discoveryContext">discovery context settings</param>
        /// <param name="logger"></param>
        /// <param name="discoverySink">Unit test framework Sink</param>
        /// <remarks>Entry point of the discovery procedure</remarks>
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
#if DEBUG && LAUNCH_DEBUGGER
            System.Diagnostics.Debugger.Launch();
#endif

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(discoveryContext);

            BoostTestDiscovererFactoryOptions options = new BoostTestDiscovererFactoryOptions();
            options.ExternalTestRunnerSettings = settings.ExternalTestRunner;

            try
            {
                Logger.Initialize(logger);

                var sourceGroups = sources.GroupBy(Path.GetExtension);

                foreach (IGrouping<string, string> sourceGroup in sourceGroups)
                {
                    IBoostTestDiscoverer discoverer = TestDiscovererFactory.GetTestDiscoverer(sourceGroup.Key, options);

                    if (discoverer != null)
                    {
                        discoverer.DiscoverTests(sourceGroup, discoveryContext, logger, discoverySink);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception caught while discovering tests: {0} ({1})", ex.Message, ex.HResult);
                Logger.Error(ex.StackTrace);
            }
            finally
            {
                Logger.Shutdown();
            }
        }

        #endregion ITestDiscoverer

    }
}
