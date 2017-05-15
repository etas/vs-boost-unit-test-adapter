// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Collections.Generic;

using FakeItEasy;

using NUnit.Framework;

using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.TestBatch;
using BoostTestAdapter.Settings;

using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using BoostTestAdapter;
using BoostTestAdapter.Utility.VisualStudio;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BatchStrategyTest
    {
        #region Test Setup/Teardown

        /// <summary>
        /// Test Setup
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            RunnerFactory = A.Fake<IBoostTestRunnerFactory>();
            A.CallTo(() => RunnerFactory.GetRunner(A<string>._, A<BoostTestRunnerFactoryOptions>._)).Returns(A.Fake<IBoostTestRunner>());

            Settings = new BoostTestAdapterSettings();

            ArgsBuilder = (string source, BoostTestAdapterSettings settings) => { return new BoostTestRunnerCommandLineArgs(); };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Dummy/Default Test Runner Provider
        /// </summary>
        private IBoostTestRunnerFactory RunnerFactory { get; set; }

        /// <summary>
        /// Dummy/Default Test Adapter Settings
        /// </summary>
        private BoostTestAdapterSettings Settings { get; set; }

        /// <summary>
        /// Dummy/Default Boost.Test Command Line Arguments Builder
        /// </summary>
        private CommandLineArgsBuilder ArgsBuilder { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Given a VSTestCase as a template, generates up to count test instances
        /// based on the provided template with a unique name for each
        /// </summary>
        /// <param name="template">The template test case instance to base the generated instances upon</param>
        /// <param name="count">The total number of dummy test cases to generate</param>
        /// <returns>A collection of dummy test cases based on the provided template</returns>
        private IEnumerable<VSTestCase> GenerateDummyTests(VSTestCase template, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                var test = new VSTestCase(template.FullyQualifiedName, template.ExecutorUri, template.Source);
                
                foreach (var property in template.Properties)
                {
                    test.SetPropertyValue(property, template.GetPropertyValue(property));
                }

                // Add a suffix to allow tests to be distinguishable
                test.FullyQualifiedName += '_' + i.ToString();

                yield return test;
            }
        }

        #endregion

        #region Tests

        /// <summary>
        /// Assert that: Given a series of tests from a Boost 1.63 module/source, the One-Shot test
        ///              strategy batches all tests in 1 test run
        /// </summary>
        [Test]
        public void OneShotBatchTestStrategy()
        {
            var strategy = new OneShotTestBatchStrategy(RunnerFactory, Settings, ArgsBuilder);

            var template = new VSTestCase("test", BoostTestExecutor.ExecutorUri, "source");
            template.SetPropertyValue(VSTestModel.VersionProperty, "1.63.0");
            template.Traits.Add(VSTestModel.TestSuiteTrait, "Master Test Suite");

            var tests = GenerateDummyTests(template, 10).ToList();
            var batch = strategy.BatchTests(tests).ToList();

            Assert.That(batch.Count, Is.EqualTo(1));

            var run = batch.First();

            Assert.That(run.Tests, Is.EqualTo(tests));
            // --run_test should be avoided due to backwards compatibility
            Assert.That(run.Arguments.Tests, Is.Empty);
            // instead, the respective environment variable BOOST_TEST_RUN_FILTERS is to be used
            Assert.That(run.Arguments.Environment.ContainsKey("BOOST_TEST_RUN_FILTERS"), Is.True);
        }

        /// <summary>
        /// Assert that: Given a series of tests from a Boost 1.60 module/source, the One-Shot test
        ///              strategy batches all tests as if the TestSuite batch strategy is used
        /// </summary>
        [Test]
        public void OneShotBatchTestStrategyForOldBoost()
        {
            var strategy = new OneShotTestBatchStrategy(RunnerFactory, Settings, ArgsBuilder);

            var template = new VSTestCase("test", BoostTestExecutor.ExecutorUri, "source");
            template.SetPropertyValue(VSTestModel.VersionProperty, null);
            template.Traits.Add(VSTestModel.TestSuiteTrait, "Master Test Suite");

            var template2 = new VSTestCase("test", BoostTestExecutor.ExecutorUri, "source");
            template2.SetPropertyValue(VSTestModel.VersionProperty, null);
            template2.Traits.Add(VSTestModel.TestSuiteTrait, "suite");

            var tests = GenerateDummyTests(template, 2).Concat(GenerateDummyTests(template2, 2)).ToList();
            var batch = strategy.BatchTests(tests).ToList();
            
            // 2 test runs, 1 for "Master Test Suite" and 1 for "suite"
            Assert.That(batch.Count, Is.EqualTo(2));
            
            foreach (var run in batch)
            {
                // The --run_test argument is to be used instead of the "BOOST_TEST_RUN_FILTERS" environment variable
                Assert.That(run.Arguments.Tests, Is.Not.Empty);
                Assert.That(run.Arguments.Environment.ContainsKey("BOOST_TEST_RUN_FILTERS"), Is.False);
            }
        }

        #endregion
    }
}