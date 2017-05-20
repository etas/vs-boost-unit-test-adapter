// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BoostTestAdapter.Utility.ExecutionContext
{
    /// <summary>
    /// Aggregations of process startup settings
    /// </summary>
    public class ProcessExecutionContextArgs
    {
        /// <summary>
        /// The process file path
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The process' working directory
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Command line arguments
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Managed environment in which to execute the process
        /// </summary>
        public IDictionary<string, string> EnvironmentVariables { get; set; }
    }
    
    /// <summary>
    /// An execution context for spawning managed sub-processes
    /// </summary>
    public interface IProcessExecutionContext : IDisposable
    {
        /// <summary>
        /// Launches a new sub-process based on the provided arguments
        /// </summary>
        /// <param name="args">Process startup configuration</param>
        /// <returns>The newly launched process or null if one cannot be spawned</returns>
        Process LaunchProcess(ProcessExecutionContextArgs args);
    }
}
