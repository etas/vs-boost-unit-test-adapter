// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace BoostTestAdapter
{
    /// <summary>
    /// Interface to an object that discoverers tests.
    /// </summary>
    public interface IBoostTestDiscoverer
    {
        /// <summary>
        /// Discoverers test in the sources if supported by the discoverer.
        /// </summary>
        /// <param name="sources">The list of sources to be analysed.</param>
        /// <param name="discoveryContext">The discovery context.</param>
        /// <param name="discoverySink">The discovery sink where all the found test should be added.</param>
        void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, ITestCaseDiscoverySink discoverySink);
    }
}