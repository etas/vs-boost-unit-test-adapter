// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;

using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter.TestBatch
{
    /// <summary>
    /// An ITestBatchingStrategy which allocates a test run for test suites. All tests
    /// contained within the same test suite are executed in one test run.
    /// </summary>
    public class TestSuiteTestBatchStrategy : TestBatchStrategy
    {
        public TestSuiteTestBatchStrategy(IBoostTestRunnerFactory testRunnerFactory, BoostTestAdapterSettings settings, CommandLineArgsBuilder argsBuilder) :
            base(testRunnerFactory, settings, argsBuilder)
        {
        }

        #region TestBatchStrategy

        public override IEnumerable<TestRun> BatchTests(IEnumerable<VSTestCase> tests)
        {
            BoostTestRunnerSettings adaptedSettings = this.Settings.TestRunnerSettings.Clone();
            adaptedSettings.Timeout = -1;

            // Group by source
            IEnumerable<IGrouping<string, VSTestCase>> sources = tests.GroupBy(test => test.Source);
            foreach (IGrouping<string, VSTestCase> source in sources)
            {
                IBoostTestRunner runner = GetTestRunner(source.Key);
                if (runner == null)
                {
                    continue;
                }

                // Group by test suite
                var suiteGroups = source.GroupBy(test => test.Traits.First(trait => (trait.Name == VSTestModel.TestSuiteTrait)).Value);
                foreach (var suiteGroup in suiteGroups)
                {
                    BoostTestRunnerCommandLineArgs args = BuildCommandLineArgs(source.Key);
                    foreach (VSTestCase test in suiteGroup)
                    {
                        // List all tests by name but ensure that the first test is fully qualified so that remaining tests are taken relative to this test suite
                        args.Tests.Add((args.Tests.Count == 0) ? test.FullyQualifiedName : QualifiedNameBuilder.FromString(test.FullyQualifiedName).Peek());
                    }

                    yield return new TestRun(runner, suiteGroup, args, adaptedSettings);
                }
            }
        }

        #endregion TestBatchStrategy
    }
}