// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;

using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter.TestBatch
{
    /// <summary>
    /// Identifies a test batching strategy
    /// </summary>
    public enum Strategy
    {
        Source,
        TestSuite,
        TestCase,
        One
    }

    /// <summary>
    /// Factory function which populates a BoostTestRunnerCommandLineArgs based on the provided source and settings
    /// </summary>
    /// <param name="source">The test module source path</param>
    /// <param name="settings">Adapter settings which are currently in use</param>
    /// <returns>A BoostTestRunnerCommandLineArgs populated based on the provided information</returns>
    public delegate BoostTestRunnerCommandLineArgs CommandLineArgsBuilder(string source, BoostTestAdapterSettings settings);

    /// <summary>
    /// Base utility class for ITestBatchingStrategy implementations.
    /// </summary>
    public abstract class TestBatchStrategy : ITestBatchingStrategy
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="testRunnerFactory">Factory which provides a test runner for a specific test source</param>
        /// <param name="settings">Adapter settings which are currently in use</param>
        /// <param name="argsBuilder">Factory function which populates a BoostTestRunnerCommandLineArgs based on the provided source and settings</param>
        protected TestBatchStrategy(IBoostTestRunnerFactory testRunnerFactory, BoostTestAdapterSettings settings, CommandLineArgsBuilder argsBuilder)
        {
            this.TestRunnerFactory = testRunnerFactory;
            this.Settings = settings;
            this.ArgsBuilder = argsBuilder;
        }

        protected BoostTestAdapterSettings Settings { get; private set; }

        private IBoostTestRunnerFactory TestRunnerFactory { get; set; }
        private CommandLineArgsBuilder ArgsBuilder { get; set; }

        #region TestBatchStrategy

        public abstract IEnumerable<TestRun> BatchTests(IEnumerable<VSTestCase> tests);

        #endregion TestBatchStrategy

        /// <summary>
        /// Factory function which returns an appropriate IBoostTestRunner
        /// for the provided source or null if not applicable.
        /// </summary>
        /// <param name="source">The test source for which to retrieve the IBoostTestRunner</param>
        /// <returns>An IBoostTestRunner valid for the provided source or null if none are available</returns>
        protected IBoostTestRunner GetTestRunner(string source)
        {
            BoostTestRunnerFactoryOptions options = ((this.Settings == null) ? new BoostTestRunnerFactoryOptions() : this.Settings.TestRunnerFactoryOptions);
            return this.TestRunnerFactory.GetRunner(source, options);
        }

        /// <summary>
        /// Generates a BoostTestRunnerCommandLineArgs for the specified source
        /// </summary>
        /// <param name="source">The test module source path</param>
        /// <returns>A BoostTestRunnerCommandLineArgs populated based on the provided information</returns>
        protected BoostTestRunnerCommandLineArgs BuildCommandLineArgs(string source)
        {
            return this.ArgsBuilder(source, this.Settings);
        }
    }
}
