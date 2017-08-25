// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.ExecutionContext;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// An IBoostTestRunner wrapper which overrides the capabilities
    /// of the underlying Boost.Test runner
    /// </summary>
    public class BoostTestRunnerCapabilityOverride : IBoostTestRunner
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="runner">The underlying Boost.Test runner</param>
        /// <param name="capabilities">The overriden capability set of the Boost.Test runner</param>
        public BoostTestRunnerCapabilityOverride(IBoostTestRunner runner, IBoostTestRunnerCapabilities capabilities)
        {
            Code.Require(runner, "runner");
            Code.Require(capabilities, "capabilities");

            Runner = runner;
            Capabilities = capabilities;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Underlying Boost.Test runner instance
        /// </summary>
        public IBoostTestRunner Runner { get; private set; }

        #endregion

        #region IBoostTestRunner
        
        public IBoostTestRunnerCapabilities Capabilities { get; private set; }

        public string Source => Runner.Source;

        public int Execute(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings, IProcessExecutionContext executionContext)
        {
            return Runner.Execute(args, settings, executionContext);
        }

        #endregion
    }
}