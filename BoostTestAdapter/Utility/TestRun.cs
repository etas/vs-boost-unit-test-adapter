// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using BoostTestAdapter.Boost.Runner;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Abstraction for a Boost Test execution run.
    /// Aggregates the necessary data structures in one convenient location.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class TestRun
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="runner">The IBoostTestRunner which will be used to run the tests</param>
        /// <param name="tests">The Visual Studio test cases which will be executed</param>
        /// <param name="args">The command-line arguments for the IBoostTestRunner representing the Visual Studio test cases</param>
        /// <param name="settings">Additional settings required for correct configuration of the test runner</param>
        public TestRun(IBoostTestRunner runner, IEnumerable<VSTestCase> tests, BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
        {
            this.Runner = runner;
            this.Tests = tests;
            this.Arguments = args;
            this.Settings = settings;
        }

        public IBoostTestRunner Runner { get; private set; }

        public string Source
        {
            get { return this.Runner.Source; }
        }

        public IEnumerable<VSTestCase> Tests { get; private set; }

        public BoostTestRunnerCommandLineArgs Arguments { get; private set; }
        public BoostTestRunnerSettings Settings { get; private set; }

        /// <summary>
        /// Executes the contained IBoostTestRunner with the contained arguments and settings
        /// </summary>
        public void Run()
        {
            this.Runner.Run(this.Arguments, this.Settings);
        }

        /// <summary>
        /// Executes the contained IBoostTestRunner in debug mode with the contained arguments and settings
        /// </summary>
        /// <param name="frameworkHandle">The Visual Studio framework handle which allows for program attaching</param>
        public void Debug(IFrameworkHandle frameworkHandle)
        {
            this.Runner.Debug(this.Arguments, this.Settings, frameworkHandle);
        }
    }
}
