using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter
{
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
        }

        public bool IsListContentSupported(string exeName)
        {
            string output;
            using (var p = new Process())
            {
                _processStartInfo.FileName = exeName;
                _processStartInfo.Arguments = "--help";
                p.StartInfo = _processStartInfo;
                p.Start();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(30000);
            }
            if (!output.Contains("--list_content"))
            {
                return false;
            }
            return true;

        }

        public string GetListContentOutput(string exeName)
        {
            // get the tests list from the output 
            string output;
            using (var p = new Process())
            {
                _processStartInfo.FileName = exeName;
                _processStartInfo.Arguments = "--list_content";
                p.StartInfo = _processStartInfo;
                p.Start();
                output = p.StandardError.ReadToEnd(); // for some reason the list content output is in the standard error
                p.WaitForExit(30000);
            }
            return output;
        }


        public IDebugHelper CreateDebugHelper(string exeName)
        {
            return new DebugHelper(exeName);
        }
    }
}
