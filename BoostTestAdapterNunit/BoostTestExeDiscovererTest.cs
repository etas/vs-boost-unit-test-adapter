using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.VisualStudio;
using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUnit.Framework;
using VisualStudioAdapter;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    internal class BoostTestExeDiscovererTest
    {
        #region Test Data

        private const string Source = "test.boostd.exe";

        private const string BoostUnitTestSample = "BoostUnitTestSample.cpp";
        private const string BoostFixtureTestSuite = "BoostFixtureTestSuite.cpp";
        private const string BoostFixtureTestCase = "BoostFixtureTestCase.cpp";
        private const string BoostUnitTestSampleRequiringUseOfFilters = "BoostUnitTestSampleRequiringUseOfFilters.cpp";

        #endregion Test Data

        #region Helper Methods

        /// <summary>
        /// Applies the discovery procedure over the dummy solution
        /// </summary>
        /// <param name="solution">A dummy solution from which to discover tests from</param>
        /// <returns>An enumeration of discovered test cases</returns>
        private IEnumerable<VSTestCase> Discover(DummySolution solution)
        {
            return Discover(solution.Provider, new string[] { solution.Source });
        }

        /// <summary>
        /// Applies the discovery procedure over the dummy solution
        /// </summary>
        /// <param name="solution">A dummy solution from which to discover tests from</param>
        /// <param name="context">The IDiscoveryContext to use</param>
        /// <returns>An enumeration of discovered test cases</returns>
        private IEnumerable<VSTestCase> Discover(DummySolution solution, IDiscoveryContext context)
        {
            return Discover(solution.Provider, new string[] { solution.Source }, context);
        }

        /// <summary>
        /// Applies the discovery procedure over the provided sources
        /// </summary>
        /// <param name="provider">An IVisualStudioInstanceProvider instance</param>
        /// <param name="sources">The sources which to discover tests from</param>
        /// <returns>An enumeration of discovered test cases</returns>
        private IEnumerable<VSTestCase> Discover(IVisualStudioInstanceProvider provider, IEnumerable<string> sources)
        {
            return Discover(provider, sources, new DefaultTestContext());
        }

        /// <summary>
        /// Applies the discovery procedure over the provided sources
        /// </summary>
        /// <param name="provider">An IVisualStudioInstanceProvider instance</param>
        /// <param name="sources">The sources which to discover tests from</param>
        /// <param name="context">The IDiscoveryContext to use</param>
        /// <returns>An enumeration of discovered test cases</returns>
        private IEnumerable<VSTestCase> Discover(IVisualStudioInstanceProvider provider, IEnumerable<string> sources, IDiscoveryContext context)
        {
            ConsoleMessageLogger logger = new ConsoleMessageLogger();
            DefaultTestCaseDiscoverySink sink = new DefaultTestCaseDiscoverySink();

            IBoostTestDiscoverer discoverer = new BoostTestExeDiscoverer(provider);
            discoverer.DiscoverTests(sources, context, logger, sink);

            return sink.Tests;
        }

        /// <summary>
        /// Asserts general test details for the test with the requested fully qualified name
        /// </summary>
        /// <param name="tests">The discovered test case enumeration</param>
        /// <param name="fqn">The fully qualified name of the test case to test</param>
        /// <param name="source">The expected test case source</param>
        /// <returns>The test case which has been tested</returns>
        private VSTestCase AssertTestDetails(IEnumerable<VSTestCase> tests, QualifiedNameBuilder fqn, string source)
        {
            VSTestCase vsTest = tests.FirstOrDefault(test => test.FullyQualifiedName == fqn.ToString());

            Assert.That(vsTest, Is.Not.Null);
            AssertTestDetails(vsTest, fqn, source);

            return vsTest;
        }

        /// <summary>
        /// Asserts general test details for the provided test case
        /// </summary>
        /// <param name="vsTest">The test case to test</param>
        /// <param name="fqn">The expected test case fully qualified name</param>
        /// <param name="source">The expected test case source</param>
        private void AssertTestDetails(VSTestCase vsTest, QualifiedNameBuilder fqn, string source)
        {
            Assert.That(vsTest, Is.Not.Null);
            Assert.That(vsTest.DisplayName, Is.EqualTo(fqn.Peek()));
            Assert.That(vsTest.ExecutorUri, Is.EqualTo(BoostTestExecutor.ExecutorUri));
            Assert.That(vsTest.Source, Is.EqualTo(source));

            string suite = fqn.Pop().ToString();
            if (string.IsNullOrEmpty(suite))
            {
                suite = QualifiedNameBuilder.DefaultMasterTestSuiteName;
            }

            Assert.That(vsTest.Traits.Where((trait) => (trait.Name == VSTestModel.TestSuiteTrait) && (trait.Value == suite)).Count(), Is.EqualTo(1));
        }

        /// <summary>
        /// Asserts source file details for the provided test case
        /// </summary>
        /// <param name="vsTest">The test case to test</param>
        /// <param name="codeFilePath">The expected source file qualified path</param>
        /// <param name="lineNumber">The expected line number for the test case</param>
        private void AssertSourceDetails(VSTestCase vsTest, string codeFilePath, int lineNumber)
        {
            if (vsTest.CodeFilePath != null)
            {
                Assert.That(vsTest.CodeFilePath, Is.EqualTo(codeFilePath));
            }

            if (lineNumber != -1)
            {
                Assert.That(vsTest.LineNumber, Is.EqualTo(lineNumber));
            }
        }

        /// <summary>
        /// Asserts test details for tests contained within the "BoostFixtureTestSuite.cpp" source file
        /// </summary>
        /// <param name="tests">The discovered test case enumeration</param>
        /// <param name="solution">The dummy solution which contains a project referencing "BoostFixtureTestSuite.cpp"</param>
        private void AssertBoostFixtureTestSuiteTestDetails(IEnumerable<VSTestCase> tests, DummySolution solution)
        {
            DummySourceFile codeFile = solution.SourceFileResourcePaths.First((source) => source.TempSourcePath.EndsWith(BoostFixtureTestSuite));
            AssertBoostFixtureTestSuiteTestDetails(tests, solution.Source, codeFile.TempSourcePath);
        }

        /// <summary>
        /// Asserts test details for tests contained within the "BoostFixtureTestSuite.cpp" source file
        /// </summary>
        /// <param name="tests">The discovered test case enumeration</param>
        /// <param name="source">The source for which "BoostFixtureTestSuite.cpp" was compiled to</param>
        /// <param name="codeFilePath">The fully qualified path for the on-disk version of "BoostFixtureTestSuite.cpp"</param>
        private void AssertBoostFixtureTestSuiteTestDetails(IEnumerable<VSTestCase> tests, string source, string codeFilePath)
        {
            VSTestCase test1 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("FixtureSuite1/BoostTest1"), source);
            AssertSourceDetails(test1, codeFilePath, 30);

            VSTestCase test2 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("FixtureSuite1/BoostTest2"), source);
            AssertSourceDetails(test2, codeFilePath, 35);

            VSTestCase test3 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("BoostTest3"), source);
            AssertSourceDetails(test3, codeFilePath, 43);

            VSTestCase test4 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("FixtureSuite2/Fixturetest_case1"), source);
            AssertSourceDetails(test4, codeFilePath, 50);

            VSTestCase testint = AssertTestDetails(tests, QualifiedNameBuilder.FromString("FixtureSuite2/TemplatedTest<int>"), source);
            AssertSourceDetails(testint, codeFilePath, 57);

            VSTestCase testlong = AssertTestDetails(tests, QualifiedNameBuilder.FromString("FixtureSuite2/TemplatedTest<long>"), source);
            AssertSourceDetails(testlong, codeFilePath, 57);

            VSTestCase testchar = AssertTestDetails(tests, QualifiedNameBuilder.FromString("FixtureSuite2/TemplatedTest<char>"), source);
            AssertSourceDetails(testchar, codeFilePath, 57);
        }

        /// <summary>
        /// Asserts test details for tests contained within the "BoostUnitTestSample.cpp" source file
        /// </summary>
        /// <param name="tests">The discovered test case enumeration</param>
        /// <param name="solution">The dummy solution which contains a project referencing "BoostUnitTestSample.cpp"</param>
        private void AssertBoostUnitTestSampleTestDetails(IEnumerable<VSTestCase> tests, DummySolution solution)
        {
            DummySourceFile codeFile = solution.SourceFileResourcePaths.First((source) => source.TempSourcePath.EndsWith(BoostUnitTestSample));
            AssertBoostUnitTestSampleTestDetails(tests, solution.Source, codeFile.TempSourcePath);
        }

        /// <summary>
        /// Asserts test details for tests contained within the "BoostUnitTestSample.cpp" source file
        /// </summary>
        /// <param name="tests">The discovered test case enumeration</param>
        /// <param name="source">The source for which "BoostUnitTestSample.cpp" was compiled to</param>
        /// <param name="codeFilePath">The fully qualified path for the on-disk version of "BoostUnitTestSample.cpp"</param>
        private void AssertBoostUnitTestSampleTestDetails(IEnumerable<VSTestCase> tests, string source, string codeFilePath)
        {
            VSTestCase test123 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Suite1/BoostUnitTest123"), source);
            AssertSourceDetails(test123, codeFilePath, 16);

            VSTestCase test1234 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Suite1/BoostUnitTest1234"), source);
            AssertSourceDetails(test1234, codeFilePath, 20);

            VSTestCase test12345 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("BoostUnitTest12345"), source);
            AssertSourceDetails(test12345, codeFilePath, 26);

            VSTestCase testint = AssertTestDetails(tests, QualifiedNameBuilder.FromString("my_test<int>"), source);
            AssertSourceDetails(testint, codeFilePath, 33);

            VSTestCase testlong = AssertTestDetails(tests, QualifiedNameBuilder.FromString("my_test<long>"), source);
            AssertSourceDetails(testlong, codeFilePath, 33);

            VSTestCase testchar = AssertTestDetails(tests, QualifiedNameBuilder.FromString("my_test<char>"), source);
            AssertSourceDetails(testchar, codeFilePath, 33);
        }

        /// <summary>
        /// Asserts test details for tests contained within the "BoostFixtureTestCase.cpp" source file
        /// </summary>
        /// <param name="tests">The discovered test case enumeration</param>
        /// <param name="source">The source for which "BoostFixtureTestCase.cpp" was compiled to</param>
        /// <param name="codeFilePath">The fully qualified path for the on-disk version of "BoostFixtureTestCase.cpp"</param>
        private void AssertBoostFixtureTestCaseTestDetails(IEnumerable<VSTestCase> tests, string source, string codeFilePath)
        {
            VSTestCase test1 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Suit1/BoostUnitTest1"), source);
            AssertSourceDetails(test1, codeFilePath, 19);

            VSTestCase test2 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Suit1/Fixturetest_case1"), source);
            AssertSourceDetails(test2, codeFilePath, 24);

            VSTestCase test3 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Suit1/Fixturetest_case2"), source);
            AssertSourceDetails(test3, codeFilePath, 30);

            VSTestCase test4 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Fixturetest_case3"), source);
            AssertSourceDetails(test4, codeFilePath, 37);
        }

        private void AssertBoostUnitTestSampleRequiringUseOfFilters(IEnumerable<VSTestCase> tests, DummySolution solution)
        {
            DummySourceFile codeFile = solution.SourceFileResourcePaths.First((source) => source.TempSourcePath.EndsWith(BoostUnitTestSampleRequiringUseOfFilters));
            AssertBoostUnitTestSampleRequiringUseOfFilters(tests, solution.Source, codeFile.TempSourcePath);
        }

        private void AssertBoostUnitTestSampleRequiringUseOfFilters(IEnumerable<VSTestCase> tests, string source,
            string codeFilePath)
        {
            VSTestCase test1 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Suite1/BoostUnitTest123"), source);
            AssertSourceDetails(test1, codeFilePath, 16);

            VSTestCase test2 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("Suite1/BoostUnitTest1234"), source);
            AssertSourceDetails(test2, codeFilePath, 20);

            VSTestCase test3 = AssertTestDetails(tests, QualifiedNameBuilder.FromString("BoostUnitTest12345"), source);
            AssertSourceDetails(test3, codeFilePath, 26);

            VSTestCase testint = AssertTestDetails(tests, QualifiedNameBuilder.FromString("my_test<int>"), source);
            AssertSourceDetails(testint, codeFilePath, 40);

            VSTestCase testlong = AssertTestDetails(tests, QualifiedNameBuilder.FromString("my_test<long>"), source);
            AssertSourceDetails(testlong, codeFilePath, 40);

            VSTestCase testchar = AssertTestDetails(tests, QualifiedNameBuilder.FromString("my_test<char>"), source);
            AssertSourceDetails(testchar, codeFilePath, 40);

            VSTestCase testConditional = AssertTestDetails(tests, QualifiedNameBuilder.FromString("BoostUnitTestConditional"), source);
            AssertSourceDetails(testConditional, codeFilePath, 54);
        }

        #endregion Helper Methods

        #region Tests

        /// <summary>
        /// Given an valid source compiled from a single test source file, the discovery process reports the found tests accordingly.
        /// 
        /// Test aims:
        ///     - Ensure that tests can be discovered from a valid .cpp file.
        /// </summary>
        [Test]
        public void DiscoverTests()
        {
            using (DummySolution solution = new DummySolution(Source, new string[] { BoostUnitTestSample }))
            {
                IEnumerable<VSTestCase> tests = Discover(solution);
                Assert.That(tests.Count(), Is.EqualTo(6));

                AssertBoostUnitTestSampleTestDetails(tests, solution);

                // NOTE BoostUnitTest123 should not be available since it is commented out
                Assert.That(tests.Any((test) => test.FullyQualifiedName == "BoostUnitTest123"), Is.False);
            }
        }

        /// <summary>
        /// Given an valid source compiled from multiple test source files, the discovery process reports the found tests accordingly.
        /// 
        /// Test aims:
        ///     - Ensure that tests can be discovered from multiple valid .cpp files.
        /// </summary>
        [Test]
        public void DiscoverTestsFromMultipleFiles()
        {
            using (DummySolution solution = new DummySolution(Source, new string[] { BoostFixtureTestSuite, BoostUnitTestSample }))
            {
                IEnumerable<VSTestCase> tests = Discover(solution);
                Assert.That(tests.Count(), Is.EqualTo(13));

                AssertBoostFixtureTestSuiteTestDetails(tests, solution);
                AssertBoostUnitTestSampleTestDetails(tests, solution);
            }
        }

        /// <summary>
        /// Given valid sources compiled from multiple test source files, the discovery process reports the found tests accordingly.
        /// 
        /// Test aims:
        ///     - Ensure that tests can be discovered from multiple sources.
        ///     - Ensure that tests can be discovered from 'complex' project structures which include folders and other source file types.
        /// </summary>
        [Test]
        public void DiscoverTestsFromMultipleSources()
        {
            const string boostFixtureTestSuiteSource = "BoostFixtureTestSuite.boostd.exe";
            const string boostUnitTestSampleSource = "BoostUnitTestSample.boostd.exe";

            using (DummySourceFile boostFixtureTestSuiteCodeFile = new DummySourceFile(BoostFixtureTestSuite))
            using (DummySourceFile boostFixtureTestCaseCodeFile = new DummySourceFile(BoostFixtureTestCase))
            using (DummySourceFile boostUnitTestSampleSourceCodeFile = new DummySourceFile(BoostUnitTestSample))
            {
                IVisualStudio vs = new FakeVisualStudioInstanceBuilder().
                    Solution(
                        new FakeSolutionBuilder().
                            Name("SampleSolution").
                            Project(
                                new FakeProjectBuilder().
                                    Name("FixtureSampleProject").
                                    PrimaryOutput(boostFixtureTestSuiteSource).
                                    Sources(
                                            new List<string>()
                                            {
                                                boostFixtureTestSuiteCodeFile.TempSourcePath,
                                                boostFixtureTestCaseCodeFile.TempSourcePath
                                            })
                            ).
                            Project(
                                new FakeProjectBuilder().
                                    Name("SampleProject").
                                    PrimaryOutput(boostUnitTestSampleSource).
                                    Sources(
                                            new List<string>()
                                            {
                                                boostUnitTestSampleSourceCodeFile.TempSourcePath,
                                            })
                            )
                    ).Build();

                IEnumerable<VSTestCase> vsTests = Discover(new DummyVSProvider(vs), new string[] { boostFixtureTestSuiteSource, boostUnitTestSampleSource });
                Assert.That(vsTests.Count(), Is.EqualTo(17));

                AssertBoostFixtureTestSuiteTestDetails(vsTests, boostFixtureTestSuiteSource, boostFixtureTestSuiteCodeFile.TempSourcePath);
                AssertBoostFixtureTestCaseTestDetails(vsTests, boostFixtureTestSuiteSource, boostFixtureTestCaseCodeFile.TempSourcePath);
                AssertBoostUnitTestSampleTestDetails(vsTests, boostUnitTestSampleSource, boostUnitTestSampleSourceCodeFile.TempSourcePath);
            }
        }

        /// <summary>
        /// The scope of this test is to check the correct discovery of tests when the source file contains:
        /// 1) Code that is commented (both single and multiline)
        /// 2) Boost UTF macros that might be as part of literal strings (and that hence should be filtered out)
        /// 3) Code that its inclusion is controlled by some type of conditional inclusion
        /// </summary>
        [Test]
        public void DiscoverTestsFromSourceFileRequiringUseOfFilters()
        {
            using (DummySolution solution = new DummySolution(Source, new string[] { BoostUnitTestSampleRequiringUseOfFilters }))
            {
                IEnumerable<VSTestCase> vsTests = Discover(solution);
                Assert.That(vsTests.Count(), Is.EqualTo(7));
                AssertBoostUnitTestSampleRequiringUseOfFilters(vsTests, solution);
            }
        }

        /// <summary>
        /// Given valid sources containing conditionally compiled tests, based on the .runsettings configuration, tests may still be discovered.
        /// 
        /// Test aims:
        ///     - Ensure that conditionally compiled tests are still discovered if configured to do so via the .runsettings configuration.
        /// </summary>
        [Test]
        public void DiscoverTestsUsingRunSettings()
        {
            using (DummySolution solution = new DummySolution(Source, new string[] { BoostUnitTestSampleRequiringUseOfFilters }))
            {
                DefaultTestContext context = new DefaultTestContext();
                context.RegisterSettingProvider(BoostTestAdapterSettings.XmlRootName, new BoostTestAdapterSettingsProvider());
                context.LoadEmbeddedSettings("BoostTestAdapterNunit.Resources.Settings.conditionalIncludesDisabled.runsettings");

                IEnumerable<VSTestCase> vsTests = Discover(solution, context);

                Assert.That(vsTests.Count(), Is.EqualTo(8));
                AssertBoostUnitTestSampleRequiringUseOfFilters(vsTests, solution);

                VSTestCase testConditional = AssertTestDetails(vsTests, QualifiedNameBuilder.FromString("BoostUnitTestShouldNotAppear3"), Source);
                AssertSourceDetails(testConditional, solution.SourceFileResourcePaths.First().TempSourcePath, 47);
            }
        }

        #endregion Tests
    }
}