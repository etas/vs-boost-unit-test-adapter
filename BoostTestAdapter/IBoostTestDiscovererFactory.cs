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
        /// Returns an IBoostTestDiscoverer based on the provided source.
        /// </summary>
        /// <param name="source">Source to be associated.</param>
        /// <param name="options">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>An IBoostTestDiscoverer instance or null if one cannot be provided.</returns>
        IBoostTestDiscoverer GetDiscoverer(string source, BoostTestAdapterSettings options);

        /// <summary>
        /// Associates each source with the correct IBoostTestDiscoverer implementation.
        /// </summary>
        /// <param name="sources">List of the sources to be associated.</param>
        /// <param name="settings">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>A dictionary that has an instance of IBoostTestDiscoverer as key. Each value is a list of sources that should be analyzed with the relative discoverer.</returns>
        IEnumerable<FactoryResult> GetDiscoverers(IReadOnlyCollection<string> sources,
            BoostTestAdapterSettings settings);
    }

    public class FactoryResult
    {
        public IBoostTestDiscoverer Discoverer { get; set; }
        public IReadOnlyCollection<string> Sources { get; set; }
    }
}