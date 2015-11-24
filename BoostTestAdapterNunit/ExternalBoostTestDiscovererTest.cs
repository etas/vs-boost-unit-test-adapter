// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class ExternalBoostTestDiscovererTest
    {
        #region Helper Methods

        /// <summary>
        /// Asserts general test properties
        /// </summary>
        /// <param name="tests">The enumeration of discovered tests</param>
        /// <param name="qualifiedName">The qualified test name which is to be tested</param>
        /// <param name="source">The source from which the test should have been discovered</param>
        /// <param name="info">Optional source file information related to the test under question</param>
        private void AssertVSTestCaseProperties(IEnumerable<VSTestCase> tests, QualifiedNameBuilder qualifiedName, string source, SourceFileInfo info)
        {
            VSTestCase test = tests.FirstOrDefault((_test) => (_test.FullyQualifiedName == qualifiedName.ToString()));

            Assert.That(test, Is.Not.Null);
            Assert.That(test.DisplayName, Is.EqualTo(qualifiedName.Peek()));
            Assert.That(test.Source, Is.EqualTo(source));
            Assert.That(test.ExecutorUri, Is.EqualTo(BoostTestExecutor.ExecutorUri));

            if (info != null)
            {
                Assert.That(test.CodeFilePath, Is.EqualTo(info.File));
                Assert.That(test.LineNumber, Is.EqualTo(info.LineNumber));
            }

            Assert.That(test.Traits.Count(), Is.EqualTo(1));

            Trait trait = test.Traits.First();
            Assert.That(trait.Name, Is.EqualTo(VSTestModel.TestSuiteTrait));

            string suite = qualifiedName.Pop().ToString();
            if (string.IsNullOrEmpty(suite))
            {
                suite = qualifiedName.MasterTestSuite;
            }

            Assert.That(trait.Value, Is.EqualTo(suite));
        }

        #endregion Helper Methods

        #region Tests

        /// <summary>
        /// External test discovery based on static test listings
        /// 
        /// Test aims:
        ///     - Ensure that if configured to use static test listing, test discovery lists only the tests which can be mapped to a valid test source.
        /// </summary>
        [Test]
        public void DiscoveryFileMapDiscovery()
        {
            string listing = TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.TestLists", "sample.test.list.xml", Path.GetTempPath());

            try
            {
                ExternalBoostTestRunnerSettings settings = new ExternalBoostTestRunnerSettings
                {
                    ExtensionType = ".dll",
                    DiscoveryMethodType = DiscoveryMethodType.DiscoveryFileMap
                };

                settings.DiscoveryFileMap["test_1.dll"] = listing;

                ExternalBoostTestDiscoverer discoverer = new ExternalBoostTestDiscoverer(settings);

                DefaultTestContext context = new DefaultTestContext();
                ConsoleMessageLogger logger = new ConsoleMessageLogger();
                DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

                const string mappedSource = "C:\\test_1.dll";
                const string unmappedSource = "C:\\test_2.dll";

                discoverer.DiscoverTests(new string[] { mappedSource, unmappedSource }, context, logger, sink);

                // A total of 7 tests should be discovered as described in the Xml descriptor
                Assert.That(sink.Tests.Count(), Is.EqualTo(7));

                // All of the discovered tests should originate from C:\test_1.dll.
                // No mapping to C:\test_2.dll exist so no tests should be discovered from that source.
                Assert.That(sink.Tests.Count((test) => test.Source == mappedSource), Is.EqualTo(7));

                const string masterTestSuite = "Test runner test";

                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "test1"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 26));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "test2"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 35));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "SampleSuite/SampleNestedSuite/test3"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 48));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<char>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<int>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<float>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<double>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
            }
            finally
            {
                if (File.Exists(listing))
                {
                    File.Delete(listing);
                }
            }
        }

        /// <summary>
        /// External test discovery based on static test listings and an initial test listing containing invalid XML
        /// 
        /// Test aims:
        ///     - Ensure that a malformed xml file doesn't prevent the dectection of the following test sources.
        /// </summary>
        [Test]
        public void DiscoveryFileMapDiscovery()
        {
            string listing = TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.TestLists", "sample.test.list.xml", Path.GetTempPath());
            string invalid_listing = TestHelper.CopyEmbeddedResourceToDirectory("BoostTestAdapterNunit.Resources.TestLists", "invalid.test.list.xml", Path.GetTempPath());

            try
            {
                ExternalBoostTestRunnerSettings settings = new ExternalBoostTestRunnerSettings
                {
                    ExtensionType = ".dll",
                    DiscoveryMethodType = DiscoveryMethodType.DiscoveryFileMap
                };

                settings.DiscoveryFileMap["test_2.dll"] = invalid_listing;
                settings.DiscoveryFileMap["test_1.dll"] = listing;

                ExternalBoostTestDiscoverer discoverer = new ExternalBoostTestDiscoverer(settings);

                DefaultTestContext context = new DefaultTestContext();
                ConsoleMessageLogger logger = new ConsoleMessageLogger();
                DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

                const string mappedSource = "C:\\test_1.dll";
                const string invalidSource = "C:\\test_2.dll";

                discoverer.DiscoverTests(new string[] { mappedSource, invalidSource }, context, logger, sink);

                // A total of 7 tests should be discovered as described in the Xml descriptor
                Assert.That(sink.Tests.Count(), Is.EqualTo(7));

                // All of the discovered tests should originate from C:\test_1.dll.
                // No mapping to C:\test_2.dll exist so no tests should be discovered from that source.
                Assert.That(sink.Tests.Count((test) => test.Source == mappedSource), Is.EqualTo(7));

                const string masterTestSuite = "Test runner test";

                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "test1"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 26));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "test2"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 35));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "SampleSuite/SampleNestedSuite/test3"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 48));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<char>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<int>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<float>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
                AssertVSTestCaseProperties(sink.Tests, QualifiedNameBuilder.FromString(masterTestSuite, "TemplateSuite/my_test<double>"), mappedSource, new SourceFileInfo("test_runner_test.cpp", 79));
            }
            finally
            {
                if (File.Exists(listing))
                {
                    File.Delete(listing);
                }
            }
        }
        #endregion Tests
    }
}
