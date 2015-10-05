// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using BoostTestAdapter.Utility;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Abstract IBoostTestRunner specification which contains common functionality
    /// for executing external '.exe' Boost Test runners.
    /// </summary>
    public abstract class BoostTestRunnerBase : IBoostTestRunner
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="testRunnerExecutable">Path to the '.exe' file.</param>
        protected BoostTestRunnerBase(string testRunnerExecutable)
        {
            this.TestRunnerExecutable = testRunnerExecutable;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Boost Test runner '.exe' file path.
        /// </summary>
        protected string TestRunnerExecutable { get; private set; }

        #endregion Properties

        #region IBoostTestRunner

        public virtual void Debug(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings, IFrameworkHandle framework)
        {
            Utility.Code.Require(settings, "settings");

            using (Process process = Debug(framework, GetStartInfo(args, settings)))
            {
                MonitorProcess(process, settings.Timeout);
            }
        }

        public virtual void Run(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
        {
            Utility.Code.Require(settings,"settings");

            using (Process process = Run(GetStartInfo(args, settings)))
            {
                MonitorProcess(process, settings.Timeout);
            }
        }

        public virtual string Source
        {
            get { return this.TestRunnerExecutable;  }
        }

        #endregion IBoostTestRunner

        /// <summary>
        /// Monitors the provided process for the specified timeout.
        /// </summary>
        /// <param name="process">The process to monitor.</param>
        /// <param name="timeout">The timeout threshold until the process and its children should be killed.</param>
        /// <exception cref="TimeoutException">Thrown in case specified timeout threshold is exceeded.</exception>
        private static void MonitorProcess(Process process, int timeout)
        {
            process.WaitForExit(timeout);

            if (!process.HasExited)
            {
                KillProcessIncludingChildren(process);

                throw new TimeoutException(timeout);
            }
        }

        /// <summary>
        /// Starts a Process in a debug session using the provided command line arguments string.
        /// </summary>
        /// <param name="framework">The IFrameworkHandle which provides debug session handling.</param>
        /// <param name="info">The process start info which will be used to launch the external test program.</param>
        /// <returns>The newly spawned debug process.</returns>
        private static Process Debug(IFrameworkHandle framework, ProcessStartInfo info)
        {
            if (framework == null)
            {
                return Run(info);
            }

            Dictionary<string, string> environment =
                info.EnvironmentVariables.Cast<DictionaryEntry>().ToDictionary(
                    item => item.Key.ToString(),
                    item => item.Value.ToString()
                );

            int pid = framework.LaunchProcessWithDebuggerAttached(
                info.FileName,
                info.WorkingDirectory,
                info.Arguments,
                environment
            );

            // Get a process on the local computer, using the process id.
            return Process.GetProcessById(pid);
        }

        /// <summary>
        /// Starts a Process using the provided command line arguments string.
        /// </summary>
        /// <param name="info">The process start info which will be used to launch the external test program.</param>
        /// <returns>The newly spawned debug process.</returns>
        private static Process Run(ProcessStartInfo info)
        {
            // Wrap the process inside cmd.exe in so that we can redirect stdout,
            // stderr to file using a similar mechanism as available for Debug runs.
            return Process.Start(ProcessStartInfoEx.StartThroughCmdShell(info.Clone()));
        }

        /// <summary>
        /// Builds a ProcessStartInfo instance using the provided command line string.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="settings">The Boost Test runner settings currently being applied.</param>
        /// <returns>A ProcessStartInfo instance.</returns>
        protected virtual ProcessStartInfo GetStartInfo(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
        {
            Utility.Code.Require(args, "args");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = args.WorkingDirectory,
                FileName = this.TestRunnerExecutable,
                Arguments = args.ToString(),
                RedirectStandardError = false,
                RedirectStandardInput = false
            };

            return startInfo;
        }

        /// <summary>
        ///     Kills a process identified by its pid and all its children processes
        /// </summary>
        /// <param name="process">process object</param>
        /// <returns></returns>
        private static void KillProcessIncludingChildren(Process process)
        {
            Logger.Info("Finding processes spawned by process with Id [{0}]", process.Id);

            // Once the children pids are available we start killing the processes.
            // Enumerate each and every child immediately via the .toList() method.
            List<Process> children = EnumerateChildren(process).ToList();

            // Killing the main process
            if (KillProcess(process))
            {
                Logger.Error("Successfully killed process {0}.", process.Id);
            }
            else
            {
                Logger.Error("Unable to kill process {0}. Process may still be running.", process.Id);
            }

            foreach (Process child in children)
            {
                // Recurse
                KillProcessIncludingChildren(child);
            }
        }

        /// <summary>
        /// Enumerates all live children of the provided parent Process instance.
        /// </summary>
        /// <param name="process">The parent process whose live children are to be enumerated</param>
        /// <returns>An enumeration of live/active child processes</returns>
        private static IEnumerable<Process> EnumerateChildren(Process process)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessId FROM Win32_Process WHERE ParentProcessId = " + process.Id.ToString());
            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementBaseObject item in collection)
            {
                int childPid = Convert.ToInt32(item["ProcessId"]);

                Process child = null;

                try
                {
                    child = Process.GetProcessById(childPid);
                }
                catch (ArgumentException /* ex */)
                {
                    Logger.Error("Child process [{0}] does not exist.", childPid);
                    // Reset child to null so that it is not enumerated
                    child = null;
                }
                catch (Exception /* ex */)
                {
                    child = null;
                }

                if (child != null)
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Kill a process immediately
        /// </summary>
        /// <param name="process">process object</param>
        /// <returns>return true if success or false if it was not successfull</returns>
        private static bool KillProcess(Process process)
        {
            return KillProcess(process, 0);
        }

        /// <summary>
        /// Kill a process
        /// </summary>
        /// <param name="process">process object</param>
        /// <param name="killTimeout">the timeout in milliseconds to note correct process termination</param>
        /// <returns>return true if success or false if it was not successfull</returns>
        private static bool KillProcess(Process process, int killTimeout)
        {
            if (process.HasExited)
            {
                return true;
            }

            try
            {
                // If the call to the Kill method is made while the process is already terminating, a Win32Exception is thrown for Access Denied.

                process.Kill();
                return process.WaitForExit(killTimeout);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return false;
        }
    }
}