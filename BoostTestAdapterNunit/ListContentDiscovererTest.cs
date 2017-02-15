// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Discoverers;

using BoostTestAdapter.Utility.VisualStudio;

using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;

using FakeItEasy;
using NUnit.Framework;

using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using BoostTestAdapter.Utility.ExecutionContext;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class ListContentDiscovererTest
    {
        #region Fakes

        private class FakeBoostTestRunnerFactory : IBoostTestRunnerFactory
        {
            public FakeBoostTestRunnerFactory(IBoostTestRunner template)
            {
                this.TemplateRunner = template;
                this.ProvisionedRunners = new List<Tuple<string, BoostTestRunnerFactoryOptions, IBoostTestRunner>>();
            }

            #region IBoostTestRunnerFactory

            public IBoostTestRunner GetRunner(string identifier, BoostTestRunnerFactoryOptions options)
            {
                this.ProvisionedRunners.Add(Tuple.Create(identifier, options, this.TemplateRunner));
                return this.TemplateRunner;
            }

            #endregion IBoostTestRunnerFactory

            public IBoostTestRunner TemplateRunner { get; private set; }

            public IList<Tuple<string, BoostTestRunnerFactoryOptions, IBoostTestRunner>> ProvisionedRunners { get; private set; }
        }

        #endregion

        #region Helper Methods
        
        private void AssertLabelTrait(VSTestCase testCase, string label)
        {
            Assert.That(testCase, Is.Not.Null);
            Assert.That(testCase.Traits.Any((trait) => ((trait.Name == label) && (trait.Value.Length == 0))), Is.True);
        }

        #endregion Helper Methods
        
        /// <summary>
        /// List content discovery
        /// 
        /// Test aims:
        ///     - List content discovery issues a --list_content=DOT test runner execution
        /// </summary>
        [Test]
        public void ListContentSupport()
        {
            IBoostTestRunner runner = A.Fake<IBoostTestRunner>();

            string output = null;

            A.CallTo(() => runner.ListContentSupported).Returns(true);
            A.CallTo(() => runner.VersionSupported).Returns(false);
            A.CallTo(() => runner.Execute(A<BoostTestRunnerCommandLineArgs>._, A<BoostTestRunnerSettings>._, A<IProcessExecutionContext>._)).Invokes((call) =>
            {
                BoostTestRunnerCommandLineArgs args = (BoostTestRunnerCommandLineArgs) call.Arguments.First();
                if ((args.ListContent.HasValue) && (args.ListContent.Value == ListContentFormat.DOT))
                {
                    output = TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.ListContentDOT.sample.8.list.content.gv", args.StandardErrorFile);
                }             
            }).Returns(0);

            FakeBoostTestRunnerFactory factory = new FakeBoostTestRunnerFactory(runner);
            ListContentDiscoverer discoverer = new ListContentDiscoverer(factory, DummyVSProvider.Default);

            DefaultTestContext context = new DefaultTestContext();
            DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

            discoverer.DiscoverTests(new[] { "a.exe", }, context, sink);

            // Ensure proper test runner execution
            Assert.That(factory.ProvisionedRunners.Count, Is.EqualTo(1));
            foreach (IBoostTestRunner provisioned in factory.ProvisionedRunners.Select(provision => provision.Item3))
            {
                A.CallTo(() => provisioned.Execute(A<BoostTestRunnerCommandLineArgs>._, A<BoostTestRunnerSettings>._, A<IProcessExecutionContext>._)).
                    WhenArgumentsMatch((arguments) =>
                    {
                        BoostTestRunnerCommandLineArgs args = (BoostTestRunnerCommandLineArgs) arguments.First();

                        return (args.ListContent.HasValue) &&
                                (args.ListContent.Value == ListContentFormat.DOT) &&
                                (!string.IsNullOrEmpty(args.StandardErrorFile));
                    }).
                    MustHaveHappened();
            }

            // Ensure proper test discovery
            Assert.That(sink.Tests.Count, Is.EqualTo(8));

            AssertLabelTrait(sink.Tests.FirstOrDefault((vstest) => (vstest.FullyQualifiedName == "test_2")), "l1");
            AssertLabelTrait(sink.Tests.FirstOrDefault((vstest) => (vstest.FullyQualifiedName == "test_6")), "l1");
            var test_8 = sink.Tests.FirstOrDefault((vstest) => (vstest.FullyQualifiedName == "test_8"));
            AssertLabelTrait(test_8, "l1");
            AssertLabelTrait(test_8, "l2");
            AssertLabelTrait(test_8, "l3 withaspace");

            Assert.That(output, Is.Not.Null);

            // Ensure proper environment cleanup
            Assert.That(File.Exists(output), Is.False);
        }

        /// <summary>
        /// Assert that: Given a failing --list_content=DOT execution identified by a non-0 exit code, no tests are to be discovered.
        /// </summary>
        [Test]
        public void FailingExitCode()
        {
            IBoostTestRunner runner = A.Fake<IBoostTestRunner>();
                        
            A.CallTo(() => runner.ListContentSupported).Returns(true);
            A.CallTo(() => runner.VersionSupported).Returns(false);
            A.CallTo(() => runner.Execute(A<BoostTestRunnerCommandLineArgs>._, A<BoostTestRunnerSettings>._, A<IProcessExecutionContext>._)).Returns(-1073741515);

            FakeBoostTestRunnerFactory factory = new FakeBoostTestRunnerFactory(runner);
            ListContentDiscoverer discoverer = new ListContentDiscoverer(factory, DummyVSProvider.Default);

            DefaultTestContext context = new DefaultTestContext();
            DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

            discoverer.DiscoverTests(new[] { "missing-dll.exe", }, context, sink);
            
            // Ensure proper test discovery
            Assert.That(sink.Tests.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Assert that: Given a Boost.Test module which supports --version, all discovered tests are annotated accordingly
        /// </summary>
        [Test]
        public void VersionAnnotation()
        {
            IBoostTestRunner runner = A.Fake<IBoostTestRunner>();

            A.CallTo(() => runner.ListContentSupported).Returns(true);
            A.CallTo(() => runner.VersionSupported).Returns(true);
            A.CallTo(() => runner.Execute(A<BoostTestRunnerCommandLineArgs>._, A<BoostTestRunnerSettings>._, A<IProcessExecutionContext>._)).Invokes((call) =>
            {
                BoostTestRunnerCommandLineArgs args = (BoostTestRunnerCommandLineArgs) call.Arguments.First();

                // --list_content=DOT
                if ((args.ListContent.HasValue) && (args.ListContent.Value == ListContentFormat.DOT) && (!string.IsNullOrEmpty(args.StandardErrorFile)))
                {
                    TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.ListContentDOT.sample.3.list.content.gv", args.StandardErrorFile);
                }
                // --version
                else if ((args.Version) && (!string.IsNullOrEmpty(args.StandardErrorFile)))
                {
                    TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.Version.sample.version.stderr.log", args.StandardErrorFile);
                }
            }).Returns(0);

            FakeBoostTestRunnerFactory factory = new FakeBoostTestRunnerFactory(runner);
            ListContentDiscoverer discoverer = new ListContentDiscoverer(factory, DummyVSProvider.Default);

            DefaultTestContext context = new DefaultTestContext();
            DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

            discoverer.DiscoverTests(new[] { "test.exe", }, context, sink);

            // Ensure proper test discovery
            Assert.That(sink.Tests.Count, Is.Not.EqualTo(0));

            // Ensure that version property is available
            foreach (var test in sink.Tests)
            {
                var version = test.GetPropertyValue(VSTestModel.VersionProperty);
                Assert.That(version, Is.EqualTo("1.63.0"));
            }
        }

        /// <summary>
        /// Assert that: Given a Boost.Test module which does not support --version, all discovered tests do not advertise the Boost version
        /// </summary>
        [Test]
        public void NoVersionCapabilities()
        {
            IBoostTestRunner runner = A.Fake<IBoostTestRunner>();

            A.CallTo(() => runner.ListContentSupported).Returns(true);
            A.CallTo(() => runner.VersionSupported).Returns(false);
            A.CallTo(() => runner.Execute(A<BoostTestRunnerCommandLineArgs>._, A<BoostTestRunnerSettings>._, A<IProcessExecutionContext>._)).Invokes((call) =>
            {
                BoostTestRunnerCommandLineArgs args = (BoostTestRunnerCommandLineArgs)call.Arguments.First();

                // --list_content=DOT
                if ((args.ListContent.HasValue) && (args.ListContent.Value == ListContentFormat.DOT) && (!string.IsNullOrEmpty(args.StandardErrorFile)))
                {
                    TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.ListContentDOT.sample.3.list.content.gv", args.StandardErrorFile);
                }
                // --version
                else if ((args.Version) && (!string.IsNullOrEmpty(args.StandardErrorFile)))
                {
                    TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.Version.missing.version.stderr.log", args.StandardErrorFile);
                }
            }).Returns(0);

            FakeBoostTestRunnerFactory factory = new FakeBoostTestRunnerFactory(runner);
            ListContentDiscoverer discoverer = new ListContentDiscoverer(factory, DummyVSProvider.Default);

            DefaultTestContext context = new DefaultTestContext();
            DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

            discoverer.DiscoverTests(new[] { "test.exe", }, context, sink);

            // Ensure proper test discovery
            Assert.That(sink.Tests.Count, Is.Not.EqualTo(0));

            // Ensure that version property is not available
            foreach (var test in sink.Tests)
            {
                Assert.That(test.GetPropertyValue(VSTestModel.VersionProperty), Is.Null);
            }
        }
    }
}
