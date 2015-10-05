// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.Runtime.InteropServices;
using BoostTestAdapter.Settings;
using BoostTestAdapter.SourceFilter;
using BoostTestAdapter.Utility.VisualStudio;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace BoostTestAdapter
{
    /// <summary>
    /// Implementation of ITestDiscoverer for Boost Tests contained within .exe files
    /// </summary>
    public class BoostTestExeDiscoverer : IBoostTestDiscoverer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public BoostTestExeDiscoverer()
            : this(new DefaultVisualStudioInstanceProvider())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider"></param>
        public BoostTestExeDiscoverer(IVisualStudioInstanceProvider provider)
        {
            this.VSProvider = provider;
        }

        #endregion Constructors

        #region Properties

        public IVisualStudioInstanceProvider VSProvider { get; private set; }

        #endregion Properties

        #region ITestDiscoverer

        /// <summary>
        /// Find and pass all the testcases to discovery sink.
        /// </summary>
        /// <param name="sources">Test files containing testcases</param>
        /// <param name="discoveryContext">discovery context settings</param>
        /// <param name="logger"></param>
        /// <param name="discoverySink">Unit test framework Sink</param>
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext,
            IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(discoveryContext);
            var testDiscovererInternal = new BoostTestDiscovererInternal(this.VSProvider, SourceFilterFactory.Get(settings));
            IDictionary<string, ProjectInfo> solutioninfo = null;

            var numberOfAttempts = 100;

            // try several times to overcome "Application is Busy" COMException
            while (numberOfAttempts > 0)
            {
                try
                {
                    solutioninfo = testDiscovererInternal.PrepareTestCaseData(sources);
                    // set numberOfAttempts = 0, because there is no need to try again,
                    // since obviously no exception was thrown at this point
                    numberOfAttempts = 0;
                }
                catch (COMException)
                {
                    --numberOfAttempts;

                    // re-throw after all attempts have failed
                    if (numberOfAttempts == 0)
                    {
                        throw;
                    }
                }
            }

            testDiscovererInternal.GetBoostTests(solutioninfo, discoverySink);
        }
        
        #endregion ITestDiscoverer
    }
}