// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using System.Xml;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using BoostTestAdapter.TestBatch;
using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;
using BoostTestAdapterNunit.Utility.Xml;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestSettingsTest
    {
        #region Helper Methods

        /// <summary>
        /// Deserialises a BoostTestAdapterSettings instance from the provided embedded resource.
        /// </summary>
        /// <param name="path">Fully qualified path to a .runsettings Xml embedded resource</param>
        /// <returns>The deserialised BoostTestAdapterSettings</returns>
        private BoostTestAdapterSettings Parse(string path)
        {
            BoostTestAdapterSettingsProvider provider = new BoostTestAdapterSettingsProvider();

            DefaultTestContext context = new DefaultTestContext();
            context.RegisterSettingProvider(BoostTestAdapterSettings.XmlRootName, provider);
            context.LoadEmbeddedSettings(path);

            return provider.Settings;
        }

        /// <summary>
        /// Tests the contents of a BoostTestAdapterSettings instance, making sure they comply to the default expected values.
        /// </summary>
        /// <param name="settings">The BoostTestAdapterSettings instance to test</param>
        private void AssertDefaultSettings(BoostTestAdapterSettings settings)
        {
            Assert.That(settings.ExecutionTimeoutMilliseconds, Is.EqualTo(-1));
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(5000));
            Assert.That(settings.FailTestOnMemoryLeak, Is.False);
            Assert.That(settings.ConditionalInclusionsFilteringEnabled, Is.True);
            Assert.That(settings.LogLevel, Is.EqualTo(LogLevel.TestSuite));
            Assert.That(settings.ExternalTestRunner, Is.Null);
            Assert.That(settings.DetectFloatingPointExceptions, Is.False);
            Assert.That(settings.CatchSystemErrors, Is.True);
            Assert.That(settings.TestBatchStrategy, Is.EqualTo(Strategy.TestCase));
            Assert.That(settings.UseListContent, Is.True);
            Assert.That(settings.ForceListContent, Is.False);
            Assert.That(settings.WorkingDirectory, Is.Null);
            Assert.That(settings.EnableStdOutRedirection, Is.True);
            Assert.That(settings.EnableStdErrRedirection, Is.True);
            Assert.That(settings.Filters, Is.EqualTo(TestSourceFilter.Empty));
        }

        /// <summary>
        /// Compares the serialized content of the settings structure against an Xml embedded resource string.
        /// </summary>
        /// <param name="settings">The settings structure whose serialization is to be compared</param>
        /// <param name="resource">The path to an embedded resource which contains the serialized Xml content to compare against</param>
        private void Compare(BoostTestAdapterSettings settings, string resource)
        {
            XmlElement element = settings.ToXml();

            using (Stream stream = TestHelper.LoadEmbeddedResource(resource))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                XmlNode root = doc.DocumentElement.SelectSingleNode("/RunSettings/BoostTest");

                XmlComparer comparer = new XmlComparer();
                comparer.CompareXML(element, root, XmlNodeTypeFilter.DefaultFilter);
            }
        }

        #endregion Helper Methods

        #region Tests

        /// <summary>
        /// Default BoostTestAdapterSettings instance.
        /// 
        /// Test aims:
        ///     - Ensure that the default state of a BoostTestAdapterSettings complies with requirements.
        /// </summary>
        [Test]
        public void DefaultSettings()
        {
            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();
            AssertDefaultSettings(settings);
        }

        /// <summary>
        /// Deserializing a BoostTestAdapterSettings instance from a .runsettings Xml document.
        /// 
        /// Test aims:
        ///     - Ensure that BoostTestAdapterSettings can be deserialised from .runsettings files.
        /// </summary>
        [Test]
        public void ParseSampleSettings()
        {
            BoostTestAdapterSettings settings = Parse("BoostTestAdapterNunit.Resources.Settings.sample.runsettings");

            Assert.That(settings.ExecutionTimeoutMilliseconds, Is.EqualTo(600000));
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(600000));
            Assert.That(settings.FailTestOnMemoryLeak, Is.True);

            Assert.That(settings.ExternalTestRunner, Is.Not.Null);
            Assert.That(settings.ExternalTestRunner.ExtensionType.ToString(), Is.EqualTo(".dll"));
            Assert.That(settings.ExternalTestRunner.DiscoveryMethodType, Is.EqualTo(DiscoveryMethodType.DiscoveryCommandLine));
            Assert.That(settings.ExternalTestRunner.DiscoveryCommandLine.ToString(), Is.EqualTo("C:\\ExternalTestRunner.exe --test {source} --list-debug {out} "));
            Assert.That(settings.ExternalTestRunner.ExecutionCommandLine.ToString(), Is.EqualTo("C:\\ExternalTestRunner.exe --test {source} "));
        }

        /// <summary>
        /// Deserializing a semantically empty .runsettings Xml document provides a default BoostTestAdapterSettings instance.
        /// 
        /// Test aims:
        ///     - Ensure that empty .runsettings files do not hinder BoostTestAdapterSettings deserialization.
        /// </summary>
        [Test]
        public void ParseEmptySettings()
        {
            BoostTestAdapterSettings settings = Parse("BoostTestAdapterNunit.Resources.Settings.empty.runsettings");
            AssertDefaultSettings(settings);
        }

        /// <summary>
        /// BoostTestAdapterSettings can be deserialised from a simple .runsettings Xml document.
        /// 
        /// Test aims:
        ///     - Ensure that BoostTestAdapterSettings instances can be deserialised from a .runsettings file.
        /// </summary>
        [Test]
        public void ParseDefaultSettings()
        {
            BoostTestAdapterSettings settings = Parse("BoostTestAdapterNunit.Resources.Settings.default.runsettings");
            AssertDefaultSettings(settings);
        }

        /// <summary>
        /// BoostTestAdapterSettings can be deserialised from a 'complex' .runsettings Xml document.
        /// 
        /// Test aims:
        ///     - Ensure that BoostTestAdapterSettings instances can be deserialised from a relatively 'complex' .runsettings file.
        /// </summary>
        [Test]
        public void ParseComplexSettings()
        {
            BoostTestAdapterSettings settings = Parse("BoostTestAdapterNunit.Resources.Settings.sample.2.runsettings");
            Assert.That(settings.ExecutionTimeoutMilliseconds, Is.EqualTo(100));
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(100));

            Assert.That(settings.CatchSystemErrors, Is.False);
            Assert.That(settings.DetectFloatingPointExceptions, Is.True);
            Assert.That(settings.TestBatchStrategy, Is.EqualTo(Strategy.TestSuite));

            Assert.That(settings.UseListContent, Is.True);

            Assert.That(settings.Filters, Is.Not.Null);
            Assert.That(settings.Filters.Include, Is.Not.Empty);
            Assert.That(settings.Filters.Include, Is.EquivalentTo(new[] { "mytest.exe$" }));

            Assert.That(settings.Filters.Exclude, Is.Not.Empty);
            Assert.That(settings.Filters.Exclude, Is.EquivalentTo(new[] { "test.exe$" }));
        }

        /// <summary>
        /// BoostTestAdapterSettings can be serialized as an Xml fragment.
        /// 
        /// Test aims:
        ///     - Ensure that BoostTestAdapterSettings instances can be serialized to an Xml fragment.
        ///     - Ensure that the generated Xml fragment contains all of the necessary information to deserialize for later use.
        /// </summary>
        [Test]
        public void SerializeSettings()
        {
            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();

            settings.ExecutionTimeoutMilliseconds = 600000;
            settings.DiscoveryTimeoutMilliseconds = 600000;
            settings.FailTestOnMemoryLeak = true;

            settings.ExternalTestRunner = new ExternalBoostTestRunnerSettings()
            {
                DiscoveryMethodType = DiscoveryMethodType.DiscoveryCommandLine,
                DiscoveryCommandLine = new CommandLine("C:\\ExternalTestRunner.exe", "--test \"{source}\" --list-debug \"{out}\""),
                ExecutionCommandLine = new CommandLine("C:\\ExternalTestRunner.exe", "--test \"{source}\"")
            };

            Compare(settings, "BoostTestAdapterNunit.Resources.Settings.sample.runsettings");
        }

        /// <summary>
        /// Ensure that external test runner settings utilising a file map can be deserialised from a .runsettings Xml document.
        /// 
        /// Test aims:
        ///     - Ensure that external test runner settings utilising a file map can be deserialised from a .runsettings Xml document.
        /// </summary>
        [Test]
        public void ParseExternalTestRunnerDiscoveryMapSettings()
        {
            BoostTestAdapterSettings settings = Parse("BoostTestAdapterNunit.Resources.Settings.externalTestRunner.runsettings");

            Assert.That(settings.ExecutionTimeoutMilliseconds, Is.EqualTo(-1));
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(5000));
            Assert.That(settings.FailTestOnMemoryLeak, Is.False);
            Assert.That(settings.ConditionalInclusionsFilteringEnabled, Is.True);
            Assert.That(settings.LogLevel, Is.EqualTo(LogLevel.TestSuite));
            Assert.That(settings.ExternalTestRunner, Is.Not.Null);

            Assert.That(settings.ExternalTestRunner.ExtensionType.ToString(), Is.EqualTo(".dll"));
            Assert.That(settings.ExternalTestRunner.DiscoveryMethodType, Is.EqualTo(DiscoveryMethodType.DiscoveryFileMap));

            IDictionary<string, string> fileMap = new Dictionary<string, string>
            {
                {"test_1.dll", "C:\\tests\\test_1.xml"},
                {"test_2.dll", "C:\\tests\\test_2.xml"}
            };

            Assert.That(settings.ExternalTestRunner.DiscoveryFileMap, Is.EqualTo(fileMap));
            Assert.That(settings.ExternalTestRunner.ExecutionCommandLine.ToString(), Is.EqualTo("C:\\ExternalTestRunner.exe --test {source} "));
        }

        /// <summary>
        /// Ensure that external test runner settings utilitsing a file map can be deserialised from a .runsettings Xml document.
        /// 
        /// Test aims:
        ///     - Ensure that external test runner settings utilitsing a file map can be deserialised from a .runsettings Xml document.
        /// </summary>
        [Test]
        public void SerialiseExternalTestRunnerDiscoveryMapSettings()
        {
            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();

            settings.ExternalTestRunner = new ExternalBoostTestRunnerSettings
            {
                DiscoveryMethodType = DiscoveryMethodType.DiscoveryFileMap,
                ExecutionCommandLine = new CommandLine("C:\\ExternalTestRunner.exe", "--test \"{source}\"")
            };

            settings.ExternalTestRunner.DiscoveryFileMap.Add("test_1.dll", "C:\\tests\\test_1.xml");
            settings.ExternalTestRunner.DiscoveryFileMap.Add("test_2.dll", "C:\\tests\\test_2.xml");

            Compare(settings, "BoostTestAdapterNunit.Resources.Settings.externalTestRunner.runsettings");
        }

        #endregion Tests
    }
}
