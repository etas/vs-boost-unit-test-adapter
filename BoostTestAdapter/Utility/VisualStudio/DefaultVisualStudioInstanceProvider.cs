// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Text.RegularExpressions;
using EnvDTE80;

namespace BoostTestAdapter.Utility.VisualStudio
{
    /// <summary>
    /// Default implementation of an IVisualStudioInstanceProvider. Provides an IVisualStudio instance based on currently running Visual Studio IDE instances.
    /// </summary>
    public class DefaultVisualStudioInstanceProvider : IVisualStudioInstanceProvider
    {
        /// <summary>
        /// Default prefix for VisualStudio DTE monickers
        /// </summary>
        private const string VisualStudioDTEPrefix = "!VisualStudio.DTE.";

        /// <summary>
        /// Regex to extract Visual Studio version Id
        /// </summary>
        private static readonly Regex VisualStudioDTEVersionRegex = new Regex("^" + Regex.Escape(VisualStudioDTEPrefix) + @"(\d+)", RegexOptions.IgnoreCase);

        #region IVisualStudioInstanceProvider

        public VisualStudioAdapter.IVisualStudio Instance
        {
            get
            {
                string processId = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
                string parentProcessId = GetParentProcessId(processId);

                DTEInstance dte = GetSolutionObject(parentProcessId);

                if ((dte != null) && (dte.DTE != null))
                {
                    switch (dte.Version)
                    {
                        case "11":
                            return new VisualStudio2012Adapter.VisualStudio(dte.DTE);
                        case "12":
                            return new VisualStudio2013Adapter.VisualStudio(dte.DTE);
                        case "14":
                            return new VisualStudio2015Adapter.VisualStudio(dte.DTE);
                    }
                }

                return null;
            }
        }

        #endregion IVisualStudioInstanceProvider

        /// <summary>
        ///     Get a table of the currently running instances of the Visual Studio .NET IDE.
        /// </summary>
        /// <param name="processId">
        ///     Only return instances
        ///     that have opened a solution
        /// </param>
        /// <returns>
        ///     The DTE object corresponding to the name of the IDE
        ///     in the running object table
        /// </returns>
        private static DTEInstance GetSolutionObject(string processId)
        {
            foreach (KeyValuePair<string, object> entry in NativeMethods.RunningObjectTable)
            {
                var candidateName = entry.Key;

                if (candidateName.StartsWith(VisualStudioDTEPrefix, StringComparison.OrdinalIgnoreCase) &&
                    candidateName.EndsWith(processId, StringComparison.OrdinalIgnoreCase))
                {
                    return new DTEInstance
                    {
                        DTE = entry.Value as DTE2,
                        Version = GetVersion(candidateName)
                    };
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the process id of the parent process.
        /// </summary>
        /// <param name="processId">The process id of the child process.</param>
        /// <returns></returns>
        private static string GetParentProcessId(string processId)
        {
            string query = "SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = " + processId;
            uint parentId;

            using (ManagementObjectSearcher search = new ManagementObjectSearcher("root\\CIMV2", query))
            {
                ManagementObjectCollection.ManagementObjectEnumerator results = search.Get().GetEnumerator();
                results.MoveNext();
                ManagementBaseObject queryObj = results.Current;
                parentId = (uint)queryObj["ParentProcessId"];
            }

            return parentId.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Given a Visual Studio DTE monicker, extracts the version Id from the name
        /// </summary>
        /// <param name="candidateName">The Visual Studio COM object display name</param>
        /// <returns>The Visual Studio COM Object version or an empty string if the version cannot be located</returns>
        private static string GetVersion(string candidateName)
        {
            Match match = VisualStudioDTEVersionRegex.Match(candidateName);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Minor class used to aggregate version information
        /// and the respective Visual Studio DTE2 instance.
        /// </summary>
        private class DTEInstance
        {
            public string Version { get; set; }

            public DTE2 DTE { get; set; }
        }
    }
}