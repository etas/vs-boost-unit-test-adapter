// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Aggregates common settings for a Boost Test execution.
    /// </summary>
    public class BoostTestRunnerSettings : ICloneable
    {
        /// <summary>
        /// Constructor. Initializes all settings to their default state.
        /// </summary>
        public BoostTestRunnerSettings()
        {
            this.Timeout = -1;
        }

        /// <summary>
        /// Timeout for unit test execution.
        /// </summary>
        public int Timeout { get; set; }

        #region IClonable

        public BoostTestRunnerSettings Clone()
        {
            return new BoostTestRunnerSettings()
            {
                Timeout = this.Timeout
            };
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion IClonable
    }
}