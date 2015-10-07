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
        public ExternalBoostTestRunnerSettings ExternalTestRunnerSettings { get; set; }
    }
}