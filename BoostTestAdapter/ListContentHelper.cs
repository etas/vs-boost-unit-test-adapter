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
        //private const string _masterTestSuiteDebugSymbolName = "boost::unit_test::framework::master_test_suite";
        private const string _listContentDebugSymbolName = "boost::unit_test::runtime_config::list_content";

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
        /// Process timeout for reading --list_content output
        /// </summary>
        public int Timeout { get; set; }
        
        private void TimeoutTimerCallback(object state)
        {
            var process = (Process)state;
            if (!process.HasExited)
                process.Kill();
        }

        #region IListContentHelper
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool IsListContentSupported(string exeName)
        {
            Code.Require(exeName, "exeName");

            // Try to locate the list_content function debug symbol. If this is not available, this implies that:
            // - Debug symbols are not available for the requested source
            // - Debug symbols are available but the source is not a Boost Unit Test version >= 3 module
            try
            {
                using (IDebugHelper dbgHelp = CreateDebugHelper(exeName))
                {
                    return dbgHelp.ContainsSymbol(_listContentDebugSymbolName);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not create a DBGHELP instance for '{0}' to determine whether symbols are available.", exeName);
            }

            return false;
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
    }
}
