// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// BoostTestRunner interface. Identifies a Boost Test Runner.
    /// </summary>
    public interface IBoostTestRunner
    {
        /// <summary>
        /// Initializes a debug instance of this Boost Test runner.
        /// </summary>
        /// <param name="args">The Boost Test framework command line options.</param>
        /// <param name="settings">The Boost Test runner settings.</param>
        /// <param name="framework">An IFrameworkHandle which provides debugging capabilities.</param>
        /// <exception cref="TimeoutException">Thrown in case specified timeout threshold is exceeded.</exception>
        void Debug(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings, IFrameworkHandle framework);

        /// <summary>
        /// Executes the Boost Test runner with the provided arguments.
        /// </summary>
        /// <param name="args">The Boost Test framework command line options.</param>
        /// <param name="settings">The Boost Test runner settings.</param>
        /// <exception cref="TimeoutException">Thrown in case specified timeout threshold is exceeded.</exception>
        void Run(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings);

        /// <summary>
        /// Provides a source Id distinguishing different instances
        /// </summary>
        string Source { get; }
    }
}