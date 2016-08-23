// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using BoostTestAdapter.Settings;

namespace BoostTestAdapter
{
    /// <summary>
    /// Abstract Factory which provides ITestDiscoverer instances.
    /// </summary>
    public interface IBoostTestDiscovererFactory
    {
        /// <summary>
        /// Associates each source with the correct IBoostTestDiscoverer implementation.
        /// </summary>
        /// <param name="sources">List of the sources to be associated.</param>
        /// <param name="settings">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>A dictionary that has an instance of IBoostTestDiscoverer as key. Each value is a list of sources that should be analyzed with the relative discoverer.</returns>
        IEnumerable<FactoryResult> GetDiscoverers(IReadOnlyCollection<string> sources, BoostTestAdapterSettings settings);
    }

    /// <summary>
    /// Resultant output of IBoostTestDiscovererFactory.GetDiscoverers
    /// </summary>
    public class FactoryResult
    {
        /// <summary>
        /// The discoverer to be used for the enclosed sources
        /// </summary>
        public IBoostTestDiscoverer Discoverer { get; set; }

        /// <summary>
        /// A collection of sources which are related to the enclosed discoverer
        /// </summary>
        public IReadOnlyCollection<string> Sources { get; set; }
    }
}