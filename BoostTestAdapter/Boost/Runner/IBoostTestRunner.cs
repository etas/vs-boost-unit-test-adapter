// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility.ExecutionContext;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// BoostTestRunner interface. Identifies a Boost Test Runner.
    /// </summary>
    public interface IBoostTestRunner
    {
        /// <summary>
        /// Executes the Boost Test runner with the provided arguments within the provided execution context.
        /// </summary>
        /// <param name="args">The Boost Test framework command line options.</param>
        /// <param name="settings">The Boost Test runner settings.</param>
        /// <param name="executionContext">An IProcessExecutionContext which will manage any spawned process.</param>
        /// <returns>Boost.Test result code</returns>
        /// <exception cref="TimeoutException">Thrown in case specified timeout threshold is exceeded.</exception>
        int Execute(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings, IProcessExecutionContext executionContext);
        
        /// <summary>
        /// Provides a source Id distinguishing different instances
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Determines if the test runner provides --list_content capabilities.
        /// </summary>
        bool ListContentSupported { get; }
    }
}