// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Collections.Generic;

using BoostTestAdapter.Boost.Runner;

using BoostTestAdapter.Settings;

using VisualStudioAdapter;

namespace BoostTestAdapter.Utility
{
    public static class BoostTestRunnerCommandLineArgsEx
    {
        /// <summary>
        /// Allows specification of an environment via a line separated string
        /// </summary>
        /// <param name="args">The arguments to populate</param>
        /// <param name="environment">The line separated environment string</param>
        public static void SetEnvironment(this BoostTestRunnerCommandLineArgs args, string environment)
        {
            Code.Require(args, "args");

            if (!string.IsNullOrEmpty(environment))
            {
                foreach (string entry in environment.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] keyValuePair = entry.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if ((keyValuePair != null) && (keyValuePair.Length == 2))
                    {
                        args.Environment[keyValuePair[0]] = keyValuePair[1];
                    }
                }
            }
        }

        /// <summary>
        /// Sets the working environment (i.e. WorkingDirectory and Environment) properties of the command line arguments
        /// based on the provided details
        /// </summary>
        /// <param name="args">The arguments which to set</param>
        /// <param name="source">The base source which will be executed</param>
        /// <param name="settings">The BoostTestAdapterSettings which are currently applied</param>
        /// <param name="vs">The current Visual Studio instance (if available)</param>
        /// <exception cref="COMException"></exception>
        public static void SetWorkingEnvironment(this BoostTestRunnerCommandLineArgs args, string source, BoostTestAdapterSettings settings, IVisualStudio vs)
        {
            Code.Require(args, "args");
            Code.Require(source, "source");
            Code.Require(settings, "settings");

            // Default working directory
            args.WorkingDirectory = Path.GetDirectoryName(source);

            // Working directory extracted from test settings
            if (!string.IsNullOrEmpty(settings.WorkingDirectory) && Directory.Exists(settings.WorkingDirectory))
            {
                args.WorkingDirectory = settings.WorkingDirectory;
            }

            if (vs != null)
            {
                // Visual Studio configuration has higher priority over settings (if available)
                IVSDebugConfiguration vsConfiguration = LocateVSDebugConfiguration(source, vs);
                if (vsConfiguration != null)
                {
                    args.WorkingDirectory = vsConfiguration.WorkingDirectory;
                    args.SetEnvironment(vsConfiguration.Environment);
                }
            }
        }

        /// <summary>
        /// Locates the Visual Studio Debug configuration for the provided source
        /// </summary>
        /// <param name="source">The source to lookup its Visual Studio configuration</param>
        /// <param name="vs">The Visual Studio instance from which to search</param>
        /// <returns>The respective Visual Studio debug configuration or null if it cannot be located</returns>
        private static IVSDebugConfiguration LocateVSDebugConfiguration(string source, IVisualStudio vs)
        {
            foreach (var project in vs.Solution.Projects)
            {
                var configuration = project.ActiveConfiguration;

                if (string.Equals(source, configuration.PrimaryOutput, StringComparison.Ordinal))
                {
                    return configuration.VSDebugConfiguration;
                }
            }

            return null;
        }
    }
}
