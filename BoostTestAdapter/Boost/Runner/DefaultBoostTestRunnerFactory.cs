// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Text.RegularExpressions;

using BoostTestAdapter.Settings;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Default implementation for IBoostTestRunnerFactory.
    /// </summary>
    public class DefaultBoostTestRunnerFactory : IBoostTestRunnerFactory
    {
        #region Constants

        /// <summary>
        /// 'Fast-path' filename pattern. Such filenames are assumed to be valid Boost.Test modules which support '--list_content'.
        /// </summary>
        private static readonly Regex _forceListContentExtensionPattern = new Regex(@"test\.boost(?:d)?\.exe$", RegexOptions.IgnoreCase);

        #region Properties

        /// <summary>
        /// Boost 1.59 version identifier
        /// </summary>
        public static readonly Version Boost159 = new Version(1, 59);

        /// <summary>
        /// Boost 1.62 version identifier
        /// </summary>
        public static readonly Version Boost162 = new Version(1, 62);

        /// <summary>
        /// Boost 1.63 version identifier
        /// </summary>
        public static readonly Version Boost163 = new Version(1, 63);

        #endregion

        #endregion

        #region IBoostTestRunnerFactory

        /// <summary>
        /// Based on the provided file name, returns a suitable IBoostTestRunner
        /// instance or null if none are available.
        /// </summary>
        /// <param name="identifier">The identifier which is to be executed via the IBoostTestRunner.</param>
        /// <param name="options">test runner settings</param>
        /// <returns>A suitable IBoostTestRunner instance or null if none are available.</returns>
        public IBoostTestRunner GetRunner(string identifier, BoostTestRunnerFactoryOptions options)
        {
            IBoostTestRunner runner = null;

            if ((options != null) && (options.ExternalTestRunnerSettings != null))
            {
                // Provision an external test runner
                runner = GetExternalTestRunner(identifier, options.ExternalTestRunnerSettings);
            }

            // Provision a default internal runner
            if (runner == null)
            {
                runner = GetInternalTestRunner(identifier);
            }

            if (runner != null)
            {
                // Apply specific Boost 1.62 workaround
                if (GetBoost162Workaround(options))
                {
                    runner = new BoostTest162Runner(runner);
                }
                
                // Force the use of a specific Boost.Test runner implementation
                Version version = GetBoostTestVersion(identifier, options);

                if (version != null)
                {
                    // Assume runner capabilities based on provided version
                    var capabilities = new BoostTestRunnerCapabilities
                    {
                        ListContent = (version >= Boost159),
                        Version = (version >= Boost163)
                    };

                    runner = new BoostTestRunnerCapabilityOverride(runner, capabilities);
                }
            }

            return runner;
        }

        #endregion IBoostTestRunnerFactory

        /// <summary>
        /// Generates a suitable test runner for the provided source
        /// </summary>
        /// <param name="source">The test module file path</param>
        /// <returns>An test runner for the provided source or null if one cannot be produced</returns>
        private static IBoostTestRunner GetInternalTestRunner(string source)
        {
            switch (Path.GetExtension(source))
            {
                case ".exe": return new BoostTestRunner(source);
            }

            return null;
        }

        /// <summary>
        /// Generates a suitable external test runner for the provided source
        /// </summary>
        /// <param name="source">The test module file path</param>
        /// <param name="settings">External test runner settings</param>
        /// <returns>An external test runner for the provided source or null if one cannot be produced</returns>
        private static IBoostTestRunner GetExternalTestRunner(string source, ExternalBoostTestRunnerSettings settings)
        {
            Utility.Code.Require(settings, "settings");

            if (settings.ExtensionType.IsMatch(Path.GetExtension(source)))
            {
                return new ExternalBoostTestRunner(source, settings);
            }

            return null;
        }

        /// <summary>
        /// Acquires the Boost.Test from the settings and test module file path
        /// </summary>
        /// <param name="source">The test module file path</param>
        /// <param name="options">Test runner factory options</param>
        /// <returns>The specified Boost.Test version or null if the version cannot be assumed from the provided details</returns>
        private static Version GetBoostTestVersion(string source, BoostTestRunnerFactoryOptions options)
        {
            Version version = options?.ForcedBoostTestVersion;

            // Convention over configuration. Assume test runners utilising such an
            // extension pattern to be Boost 1.59 capable test runners.
            if ((version == null) && _forceListContentExtensionPattern.IsMatch(source))
            {
                version = Boost159;
            }

            return version;
        }

        /// <summary>
        /// Determines whether or not the Boost 1.62 workaround is to be applied
        /// </summary>
        /// <param name="options">Test runner factory options</param>
        /// <returns>true if the Boost 1.62 workaround should be applied; false otherwise</returns>
        private static bool GetBoost162Workaround(BoostTestRunnerFactoryOptions options)
        {
            return (options != null) && (options.UseBoost162Workaround || (options.ForcedBoostTestVersion == Boost162));
        }
    }
}