// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Settings;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Aggregates all options for BoostTestRunnerFactory
    /// </summary>
    public class BoostTestRunnerFactoryOptions
    {
        /// <summary>
        /// Flag determining if Boost Test available in Boost 1.62 is in use
        /// </summary>
        public bool UseBoost162Workaround { get; set; } = false;

        /// <summary>
        /// Settings for external test runner use
        /// </summary>
        public ExternalBoostTestRunnerSettings ExternalTestRunnerSettings { get; set; }
    }
}