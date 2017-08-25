// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using BoostTestAdapter.Settings;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Aggregates all options for BoostTestRunnerFactory
    /// </summary>
    public class BoostTestRunnerFactoryOptions : IEquatable<BoostTestRunnerFactoryOptions>
    {
        /// <summary>
        /// Version identifier which forces the factory to assume a Boost.Test version
        /// </summary>
        public Version ForcedBoostTestVersion { get; set; } = null;

        /// <summary>
        /// Flag determining if Boost Test available in Boost 1.62 is in use
        /// </summary>
        public bool UseBoost162Workaround { get; set; } = false;

        /// <summary>
        /// Settings for external test runner use
        /// </summary>
        public ExternalBoostTestRunnerSettings ExternalTestRunnerSettings { get; set; } = null;

        #region Object

        public override bool Equals(object obj)
        {
            return Equals(obj as BoostTestRunnerFactoryOptions);
        }

        public override int GetHashCode()
        {
            int hash = 21;

            hash = hash * 33 + ((ForcedBoostTestVersion == null) ? 0 : ForcedBoostTestVersion.GetHashCode());
            hash = hash * 33 + UseBoost162Workaround.GetHashCode();
            hash = hash * 33 + ((ExternalTestRunnerSettings == null) ? 0 : ExternalTestRunnerSettings.GetHashCode());

            return hash;
        }

        #endregion

        #region IEquatable<BoostTestRunnerFactoryOptions>

        public bool Equals(BoostTestRunnerFactoryOptions other)
        {
            return (other != null) &&
                ((ForcedBoostTestVersion == other.ForcedBoostTestVersion) || ((ForcedBoostTestVersion != null) && ForcedBoostTestVersion.Equals(other.ForcedBoostTestVersion))) &&
                (UseBoost162Workaround == other.UseBoost162Workaround) &&
                ((ExternalTestRunnerSettings == other.ExternalTestRunnerSettings) || ((ExternalTestRunnerSettings != null) && ExternalTestRunnerSettings.Equals(other.ExternalTestRunnerSettings)));
        }

        #endregion
    }
}