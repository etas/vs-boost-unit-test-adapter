// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;

using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter.TestBatch
{
    /// <summary>
    /// An ITestBatchingStrategy implementation which provides a test run per provided source.
    /// Regardless whether or not all tests in a source are requested to executed, all tests
    /// contained within a source will be executed.
    /// </summary>
    public class SourceTestBatchStrategy : TestBatchStrategy
    {
        public SourceTestBatchStrategy(IBoostTestRunnerFactory testRunnerFactory, BoostTestAdapterSettings settings, CommandLineArgsBuilder argsBuilder) :
            base(testRunnerFactory, settings, argsBuilder)
        {
        }

        #region TestBatchStrategy

        public override IEnumerable<TestRun> BatchTests(IEnumerable<VSTestCase> tests)
        {
            BoostTestRunnerSettings adaptedSettings = this.Settings.TestRunnerSettings.Clone();
            adaptedSettings.RunnerTimeout = -1;

            // Group by source
            var sources = tests.GroupBy(test => test.Source);
            foreach (var source in sources)
            {
                IBoostTestRunner runner = GetTestRunner(source.Key);
                if (runner == null)
                {
                    continue;
                }
    
                BoostTestRunnerCommandLineArgs args = BuildCommandLineArgs(source.Key);

                // NOTE the --run_test command-line arg is left empty so that all tests are executed

                yield return new TestRun(runner, source, args, adaptedSettings);
            }
        }

        #endregion TestBatchStrategy
    }
}