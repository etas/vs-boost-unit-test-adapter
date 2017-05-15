// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Linq;
using System.Collections.Generic;

using BoostTestAdapter.Utility;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Utility.VisualStudio;

using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter.TestBatch
{
    public class OneShotTestBatchStrategy : TestBatchStrategy
    {
        #region Constants

        private static readonly Version _minimumVersion = new Version(1, 63, 0);
        private static readonly Version _zero = new Version(0, 0, 0);

        #endregion

        #region Constructors

        public OneShotTestBatchStrategy(IBoostTestRunnerFactory testRunnerFactory, BoostTestAdapterSettings settings, CommandLineArgsBuilder argsBuilder)
            : base(testRunnerFactory, settings, argsBuilder)
        {
            _fallBackStrategy = new TestSuiteTestBatchStrategy(testRunnerFactory, settings, argsBuilder);
        }

        #endregion

        #region Members

        private readonly TestSuiteTestBatchStrategy _fallBackStrategy;

        #endregion

        #region TestBatchStrategy

        public override IEnumerable<TestRun> BatchTests(IEnumerable<VSTestCase> tests)
        {
            BoostTestRunnerSettings adaptedSettings = this.Settings.TestRunnerSettings.Clone();
            // Disable timeout since this batching strategy executes more than one test at a time
            adaptedSettings.Timeout = -1;

            // Group by source
            IEnumerable<IGrouping<string, VSTestCase>> sources = tests.GroupBy(test => test.Source);
            foreach (var source in sources)
            {
                IBoostTestRunner runner = GetTestRunner(source.Key);
                if (runner == null)
                {
                    continue;
                }

                // Start by batching tests by TestSuite
                var batch = _fallBackStrategy.BatchTests(source);
                
                // If the Boost.Test module supports test run filters...
                if (source.Select(GetVersion).All(version => (version >= _minimumVersion)))
                {
                    BoostTestRunnerCommandLineArgs args = BuildCommandLineArgs(source.Key);

                    // Generate the filter set
                    var filterSet = new List<TestFilter>();

                    foreach (var run in batch)
                    {
                        TestFilter filter = TestFilter.EnableFilter();

                        // Use the command-line representation of the test suite batch to allow
                        // for the most compact representation (i.e. fully/qualified/test_name_0,test_name_1,test_name_2)
                        filter.TestSet = new PathTestSet(run.Arguments.Tests);
                        
                        filterSet.Add(filter);
                    }

                    // Use the environment variable rather than the command-line '--run_test' to make proper use of test run filters
                    args.Environment["BOOST_TEST_RUN_FILTERS"] = string.Join(":", filterSet);
                    
                    yield return new TestRun(runner, source, args, adaptedSettings);
                }
                // Else fall-back to regular test suite batching behaviour...
                else
                {
                    foreach (var run in batch)
                    {
                        yield return run;
                    }
                }                
            }
        }

        #endregion

        /// <summary>
        /// Extracts the Boost version from the provided test case
        /// </summary>
        /// <param name="test">The test case to extract the version from</param>
        /// <returns>The version advertised by the provided test case or the zero version if the version is not available</returns>
        private static Version GetVersion(VSTestCase test)
        {
            // Use the Zero version as a dummy to identify a missing Boost Version property
            Version result = _zero;

            if (test != null)
            {
                string value = (string)test.GetPropertyValue(VSTestModel.VersionProperty);

                if (string.IsNullOrEmpty(value) || !Version.TryParse(value, out result))
                {
                    result = _zero;
                }
            }

            return result;
        }

        #region Boost Test Filtering

        /// <summary>
        /// Boost.Test test filter representation
        /// </summary>
        private class TestFilter
        {
            /// <summary>
            /// Internal Constructor
            /// </summary>
            /// <param name="relativeSpec">Identifies if this is an 'enable' filter or otherwise</param>
            private TestFilter(bool relativeSpec)
            {
                this.RelativeSpec = relativeSpec;
            }

            /// <summary>
            /// States if this is an 'enable' filter or otherwise
            /// </summary>
            public bool RelativeSpec { get; private set; }

            /// <summary>
            /// The test set
            /// </summary>
            public ITestSet TestSet { get; set; }

            /// <summary>
            /// Named constructor which creates an 'enabled' test filter. The test set
            /// represented will be executed.
            /// </summary>
            /// <returns>A new TestFilter instance whose TestSet will be enabled for execution</returns>
            public static TestFilter EnableFilter()
            {
                return new TestFilter(true);
            }

            /// <summary>
            /// Named constructor which creates a 'disabled' test filter. The test set
            /// represented will not be executed.
            /// </summary>
            /// <returns>A new TestFilter instance whose TestSet will not be enabled for execution</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public static TestFilter DisableFilter()
            {
                return new TestFilter(false);
            }
            public override string ToString()
            {
                return (RelativeSpec ? '+' : '!') + this.TestSet.ToString();
            }
        }

        /// <summary>
        /// Identifies a 'test_set' as specified in Boost.Test <see cref="http://www.boost.org/doc/libs/1_63_0/libs/test/doc/html/boost_test/utf_reference/rt_param_reference/run_test.html">--run_test</see> documentation
        /// </summary>
        private interface ITestSet
        {
        }

        /// <summary>
        /// A 'label' test set which represents
        /// all tests identified with said label
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        private class LabelTestSet : ITestSet
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="label">The test label</param>
            public LabelTestSet(string label)
            {
                this.Label = label;
            }

            /// <summary>
            /// Test label (decorator) value
            /// </summary>
            public string Label { get; private set; }

            public override string ToString()
            {
                return '@' + this.Label;
            }
        }

        /// <summary>
        /// A 'path' test set which represents a collection
        /// of Boost.Test paths
        /// </summary>
        /// <remarks>The first path is expected to be fully-qualified</remarks>
        /// <remarks>Paths following the first path are relative to the parent test suite of the first fully-qualified test</remarks>
        private class PathTestSet : ITestSet
        {
            /// <summary>
            /// Default Constructor
            /// </summary>
            public PathTestSet()
                : this(new List<string>())
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="paths">The base list which is to be used</param>
            /// <remarks>The list is shallow copied. Any modifications Paths through this instance will also affect the provided list.</remarks>
            public PathTestSet(IList<string> paths)
            {
                this.Paths = paths;
            }

            /// <summary>
            /// Test paths
            /// </summary>
            public IList<string> Paths { get; private set; }

            public override string ToString()
            {
                return string.Join(",", this.Paths);
            }
        }

        #endregion
    }
}
