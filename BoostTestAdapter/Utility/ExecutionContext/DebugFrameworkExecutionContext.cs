// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace BoostTestAdapter.Utility.ExecutionContext
{
    /// <summary>
    /// An IProcessExecutionContext which produces sub-processes which are attached to Visual Studio for debugging.
    /// </summary>
    public sealed class DebugFrameworkExecutionContext : IProcessExecutionContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="framework">The IFrameworkHandle instance which provides the capabilities to attach to Visual Studio</param>
        public DebugFrameworkExecutionContext(IFrameworkHandle framework)
        {
            Utility.Code.Require(framework, "framework");

            this.Framework = framework;
        }
        
        public IFrameworkHandle Framework { get; private set; }

        #region IProcessExecutionContext

        public Process LaunchProcess(ProcessExecutionContextArgs args)
        {
            Utility.Code.Require(args, "args");

            int pid = this.Framework.LaunchProcessWithDebuggerAttached(args.FilePath, args.WorkingDirectory, args.Arguments, args.EnvironmentVariables);
            return Process.GetProcessById(pid);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {

        }

        #endregion
    }
}
