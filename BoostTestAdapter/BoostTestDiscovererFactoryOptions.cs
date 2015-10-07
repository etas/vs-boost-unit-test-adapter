// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Settings;

namespace BoostTestAdapter
{
    /// <summary>
    /// Options for Boost Test discoverer provisioning
    /// </summary>
    public class BoostTestDiscovererFactoryOptions
    {
        public ExternalBoostTestRunnerSettings ExternalTestRunnerSettings { get; set; }
    }
}
