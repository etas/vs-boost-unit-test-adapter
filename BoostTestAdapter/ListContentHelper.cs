// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Diagnostics;
using BoostTestAdapter.Utility;
using System.IO;
using System.Threading;
using BoostTestAdapter.Boost.Runner;

namespace BoostTestAdapter
{
    /// <summary>
    /// An implementation of IListContentHelper.
    /// </summary>
    class ListContentHelper : IListContentHelper
    {
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


        public int Timeout { get; set; }


        private void TimeoutTimerCallback(object state)
        {
            var process = (Process)state;
            if (!process.HasExited)
                process.Kill();
        }

        public bool IsListContentSupported(string exeName)
        {
            Code.Require(exeName, "exeName");

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
            
            // check for the presence of PDB file
            var exeDir = Path.GetDirectoryName(exeName);
            var exeNameNoExt = Path.GetFileNameWithoutExtension(exeName);

            var pdbName = exeNameNoExt + ".PDB";
            var pdbPath = Path.Combine(exeDir, pdbName);

            if (!File.Exists(pdbPath))
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

    }
}
