// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;

namespace BoostTestAdapter
{
    /// <summary>
    /// Utility extensions for IBoostTestDiscovererFactory
    /// </summary>
    public static class BoostTestDiscovererUtility
    {

        /// <summary>
        /// Returns an IBoostTestDiscoverer based on the provided source.
        /// </summary>
        /// <param name="factory">The factory to utilise.</param>
        /// <param name="source">Source to be associated.</param>
        /// <param name="settings">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>An IBoostTestDiscoverer instance or null if one cannot be provided.</returns>
        public static IBoostTestDiscoverer GetDiscoverer(this IBoostTestDiscovererFactory factory, string source, Settings.BoostTestAdapterSettings settings)
        {
            Utility.Code.Require(factory, "factory");

            var list = new[] { source };
            var results = factory.GetDiscoverers(list, settings);
            if (results != null)
            {
                var result = results.FirstOrDefault(x => x.Sources.Contains(source));
                if (result != null)
                    return result.Discoverer;
            }

            return null;
        }

    }
}
