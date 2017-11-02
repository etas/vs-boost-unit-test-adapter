// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Diagnostics;
using System.Globalization;

using JobManagement;

namespace BoostTestAdapter.Utility.ExecutionContext
{
    /// <summary>
    /// An IProcessExecutionContext which produces regular sub-processes.
    /// Guarantees that sub-processes are destroyed when this processes is killed.
    /// </summary>
    public class DefaultProcessExecutionContext : IProcessExecutionContext
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultProcessExecutionContext()
        {
            this.JobObject = new Job();
        }

        /// <summary>
        /// The JobObject which will own spawned sub-processes
        /// </summary>
        private Job JobObject { get; set; }

        #region IProcessExecutionContext

        public Process LaunchProcess(ProcessExecutionContextArgs args)
        {
            Process process = Process.Start(CreateStartInfo(args));

            if (!this.JobObject.AddProcess(process))
            {
                Logger.Warn("Process could not be added to Job Object. Test process may end up orphaned on abrupt closure.");
            }

            return process;
        }

        #endregion

        #region IDisposable

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.JobObject != null)
                    {
                        this.JobObject.Dispose();
                        this.JobObject = null;
                    }
                }
                
                disposedValue = true;
            }
        }

        ~DefaultProcessExecutionContext()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        #endregion

        /// <summary>
        /// Creates a ProcessStartInfo instance from the provided ProcessExecutionContextArgs instance
        /// </summary>
        /// <param name="args">ProcessExecutionContextArgs</param>
        /// <returns>A ProcessStartInfo which represents the provided ProcessExecutionContextArgs instance</returns>
        /// <remarks>The spawned process is executed through a cmd.exe instance to make the behavior similar to IFrameworkHandle.LaunchProcessWithDebuggerAttached</remarks>
        private static ProcessStartInfo CreateStartInfo(ProcessExecutionContextArgs args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,

                WorkingDirectory = args.WorkingDirectory,

                // Start the process through cmd.exe to allow redirection operators to work as expected
                FileName = "cmd.exe",
                Arguments = string.Format(CultureInfo.InvariantCulture, "/S /C \"{0}\"", new CommandLine(args.FilePath, args.Arguments).ToString()),

                // Redirection should be specified as part of 'args.Arguments' and sent to a file
                RedirectStandardError = false,
                RedirectStandardInput = false
            };

            if (args.EnvironmentVariables != null)
            {
                foreach (var variable in args.EnvironmentVariables)
                {
                    // Sets variable accordingly to the environment
                    if (startInfo.EnvironmentVariables.ContainsKey(variable.Key))
                    {
                        startInfo.EnvironmentVariables[variable.Key] = variable.Value;
                    }
                    else
                    {
                        startInfo.EnvironmentVariables.Add(variable.Key, variable.Value);
                    }
                }
            }

            return startInfo;
        }
    }
}
