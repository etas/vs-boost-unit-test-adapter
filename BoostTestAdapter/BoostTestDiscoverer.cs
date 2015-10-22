﻿// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BoostTestAdapter.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Discoverers;

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

        public BoostTestDiscoverer()
            :this(new BoostTestDiscovererFactory(new ListContentHelper()))
        {
        
        }

        public BoostTestDiscoverer(IBoostTestDiscovererFactory boostTestDiscovererFactory)
        {
            _boostTestDiscovererFactory = boostTestDiscovererFactory;
        }

        #endregion


        #region Members

        private readonly IBoostTestDiscovererFactory _boostTestDiscovererFactory;

        #endregion


        #region ITestDiscoverer

        /// <summary>
        /// Method called by Visual Studio (discovered via reflection) for test enumeration
        /// </summary>
        /// <param name="sources">path, target name and target extensions to discover</param>
        /// <param name="discoveryContext">discovery context settings</param>
        /// <param name="logger"></param>
        /// <param name="discoverySink">Unit test framework Sink</param>
        /// <remarks>Entry point of the discovery procedure</remarks>
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
#if DEBUG && LAUNCH_DEBUGGER
            System.Diagnostics.Debugger.Launch();
#endif

            if (sources == null)
                return;

            Logger.Initialize(logger);

            DiscoverTests(sources, discoveryContext, discoverySink);

            Logger.Shutdown();
        }

        #endregion ITestDiscoverer

        /// <summary>
        /// Method called by BoostTestExecutor for test enumeration
        /// </summary>
        /// <param name="sources">path, target name and target extensions to discover</param>
        /// <param name="discoveryContext">discovery context settings</param>
        /// <param name="discoverySink">Unit test framework Sink</param>
        /// <remarks>This method assumes that the Logger singleton is maintained by the caller.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, ITestCaseDiscoverySink discoverySink)
        {
            if (sources == null)
                return;

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(discoveryContext);

            try
            {
                var results = _boostTestDiscovererFactory.GetDiscoverers(sources.ToList(), settings);

                // Test discovery
                foreach (var d in results)
                {
                    if (d.Sources.Count > 0)
                        d.Discoverer.DiscoverTests(d.Sources, discoveryContext, Logger.Instance, discoverySink);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception caught while discovering tests: {0} ({1})", ex.Message, ex.HResult);
                Logger.Error(ex.StackTrace);
            }
        }
    }
}
