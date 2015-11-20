// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Threading;
using System.Diagnostics;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Boost.Runner;

namespace BoostTestAdapter
{
    /// <summary>
    /// An implementation of IListContentHelper.
    /// </summary>
    class ListContentHelper : IListContentHelper
    {
        private const string _masterTestSuiteDebugSymbolName = "boost::unit_test::framework::master_test_suite";

        private readonly ProcessStartInfo _processStartInfo;

        public ListContentHelper()
        {
            _processStartInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Timeout = 5000;
        }
        
        /// <summary>
        /// Process timeout for reading --help and --list_content output
        /// </summary>
        public int Timeout { get; set; }
        
        private void TimeoutTimerCallback(object state)
        {
            var process = (Process)state;
            if (!process.HasExited)
                process.Kill();
        }

        #region IListContentHelper

        public bool IsListContentSupported(string exeName)
        {
            Code.Require(exeName, "exeName");

            // Try to locate the master test suite debug symbol. If this is not available, this implies that:
            // - Debug symbols are not available for the requested source
            // - Debug symbols are available but the source is not a Boost Unit Test module
            if (!LocateMasterTestSuite(exeName))
            {
                return false;
            }
            
            // Once the master test suite is confirmed to exist, identify if the Boost Unit Test module
            // has support for '--list_content' (i.e. Boost Test version >= 3). Identify this via the '--help' output.
            var args = new BoostTestRunnerCommandLineArgs
            {
                Help = true
            };
            
            string output;
            using (var p = new Process())
            using (Timer timeoutTimer = new Timer(TimeoutTimerCallback, p, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite))
            {
                _processStartInfo.FileName = exeName;
                _processStartInfo.Arguments = args.ToString();
                p.StartInfo = _processStartInfo;
                p.Start();
                timeoutTimer.Change(Timeout, Timeout);
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(Timeout);
            }
            
            if (!output.Contains(BoostTestRunnerCommandLineArgs.ListContentArg))
            {
                return false;
            }
            
            return true;
        }

        public string GetListContentOutput(string exeName)
        {
            var args = new BoostTestRunnerCommandLineArgs
            {
                ListContent = true
            };

            // get the tests list from the output 
            string output;
            using (var p = new Process())
            using (Timer timeoutTimer = new Timer(TimeoutTimerCallback, p, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite))
            {
                _processStartInfo.FileName = exeName;
                _processStartInfo.Arguments = args.ToString();
                p.StartInfo = _processStartInfo;
                p.Start();
                timeoutTimer.Change(Timeout, Timeout);
                output = p.StandardError.ReadToEnd(); // for some reason the list content output is in the standard error
                p.WaitForExit(Timeout);
            }
            return output;
        }


        public IDebugHelper CreateDebugHelper(string exeName)
        {
            return new DebugHelper(exeName);
        }

        #endregion IListContentHelper

        /// <summary>
        /// Determines whether the executable located at the provided path contains a debug symbol
        /// for boost::unit_test::framework::master_test_suite.
        /// </summary>
        /// <param name="exeName">The path to the executable.</param>
        /// <returns>true if a symbol for the Boost Unit Test master test suite was located; false otherwise.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool LocateMasterTestSuite(string exeName)
        {
            try
            {
                using (IDebugHelper dbgHelp = CreateDebugHelper(exeName))
                {
                    return dbgHelp.ContainsSymbol(_masterTestSuiteDebugSymbolName);
                }
            }
            catch (Exception)
            {
                Logger.Warn("Could not create a DBGHELP instance for '{0}' to determine whether symbols are available.", exeName);
            }

            return false;
        }

    }
}
