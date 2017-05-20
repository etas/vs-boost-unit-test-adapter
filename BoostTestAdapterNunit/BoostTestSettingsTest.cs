// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.TestBatch;
using BoostTestAdapterNunit.Fakes;
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
        private BoostTestAdapterSettings ParseEmbeddedResource(string path)
        {
            BoostTestAdapterSettingsProvider provider = new BoostTestAdapterSettingsProvider();

            DefaultTestContext context = new DefaultTestContext();
            context.RegisterSettingProvider(BoostTestAdapterSettings.XmlRootName, provider);
            context.LoadEmbeddedSettings(path);
            
            return provider.Settings;
        }

        /// <summary>
        /// Deserialises a BoostTestAdapterSettings instance from the provided XML content.
        /// </summary>
        /// <param name="settingsXml">The settings XML content</param>
        /// <returns>The deserialised BoostTestAdapterSettings</returns>
        private BoostTestAdapterSettings ParseXml(string settingsXml)
        {
            BoostTestAdapterSettingsProvider provider = new BoostTestAdapterSettingsProvider();

            DefaultTestContext context = new DefaultTestContext();
            context.RegisterSettingProvider(BoostTestAdapterSettings.XmlRootName, provider);
            context.LoadSettings(settingsXml);

            return provider.Settings;
        }

        /// <summary>
        /// Tests the contents of a BoostTestAdapterSettings instance, making sure they comply to the default expected values.
        /// </summary>
        /// <param name="settings">The BoostTestAdapterSettings instance to test</param>
        private void AssertDefaultSettings(BoostTestAdapterSettings settings)
        {
            Assert.That(settings.ExecutionTimeoutMilliseconds, Is.EqualTo(-1));
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(30000));
            Assert.That(settings.FailTestOnMemoryLeak, Is.False);
            Assert.That(settings.LogLevel, Is.EqualTo(LogLevel.TestSuite));
            Assert.That(settings.ExternalTestRunner, Is.Null);
            Assert.That(settings.DetectFloatingPointExceptions, Is.False);
            Assert.That(settings.CatchSystemErrors, Is.Null);
            Assert.That(settings.TestBatchStrategy, Is.EqualTo(Strategy.TestCase));
            Assert.That(settings.ForceListContent, Is.False);
            Assert.That(settings.WorkingDirectory, Is.Null);
            Assert.That(settings.EnableStdOutRedirection, Is.True);
            Assert.That(settings.EnableStdErrRedirection, Is.True);
            Assert.That(settings.Filters, Is.EqualTo(TestSourceFilter.Empty));
            Assert.That(settings.RunDisabledTests, Is.False);
            Assert.That(settings.UseBoost162Workaround, Is.False);
            Assert.That(settings.PostTestDelay, Is.EqualTo(0));
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
            BoostTestAdapterSettings settings = ParseEmbeddedResource("BoostTestAdapterNunit.Resources.Settings.sample.runsettings");

            Assert.That(settings.ExecutionTimeoutMilliseconds, Is.EqualTo(600000));
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(600000));
            Assert.That(settings.FailTestOnMemoryLeak, Is.True);

            Assert.That(settings.ExternalTestRunner, Is.Not.Null);
            Assert.That(settings.ExternalTestRunner.ExtensionType.ToString(), Is.EqualTo(".dll"));
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
            BoostTestAdapterSettings settings = ParseEmbeddedResource("BoostTestAdapterNunit.Resources.Settings.empty.runsettings");
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
            BoostTestAdapterSettings settings = ParseEmbeddedResource("BoostTestAdapterNunit.Resources.Settings.default.runsettings");
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
            BoostTestAdapterSettings settings = ParseEmbeddedResource("BoostTestAdapterNunit.Resources.Settings.sample.2.runsettings");
            Assert.That(settings.ExecutionTimeoutMilliseconds, Is.EqualTo(100));
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(100));

            Assert.That(settings.CatchSystemErrors, Is.False);
            Assert.That(settings.DetectFloatingPointExceptions, Is.True);
            Assert.That(settings.TestBatchStrategy, Is.EqualTo(Strategy.TestSuite));

            Assert.That(settings.RunDisabledTests, Is.True);

            Assert.That(settings.Filters, Is.Not.Null);
            Assert.That(settings.Filters.Include, Is.Not.Empty);
            Assert.That(settings.Filters.Include, Is.EquivalentTo(new[] { "mytest.exe$" }));

            Assert.That(settings.Filters.Exclude, Is.Not.Empty);
            Assert.That(settings.Filters.Exclude, Is.EquivalentTo(new[] { "test.exe$" }));
        }
        
        /// <summary>
        /// The Boost 1.62 workaround option can be parsed
        /// 
        /// Test aims:
        ///     - Assert that: the Boost 1.62 workaround option can be parsed
        /// </summary>
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><UseBoost162Workaround>true</UseBoost162Workaround></BoostTest></RunSettings>", Result = true)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><UseBoost162Workaround>1</UseBoost162Workaround></BoostTest></RunSettings>", Result = true)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><UseBoost162Workaround>false</UseBoost162Workaround></BoostTest></RunSettings>", Result = false)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><UseBoost162Workaround>0</UseBoost162Workaround></BoostTest></RunSettings>", Result = false)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><UseBoost162Workaround /></BoostTest></RunSettings>", Result = false)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest /></RunSettings>", Result = false)]
        public bool ParseWorkaroundOption(string settingsXml)
        {
            BoostTestAdapterSettings settings = ParseXml(settingsXml);
            return settings.UseBoost162Workaround;
        }

        /// <summary>
        /// The 'PostTestDelay' option can be properly parsed
        /// 
        /// Test aims:
        ///     - Assert that: the 'PostTestDelay' option can be properly parsed
        /// </summary>
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><PostTestDelay>0</PostTestDelay></BoostTest></RunSettings>", Result = 0)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><PostTestDelay>-2147483648</PostTestDelay></BoostTest></RunSettings>", Result = -2147483648)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><PostTestDelay>2147483647</PostTestDelay></BoostTest></RunSettings>", Result = 2147483647)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><PostTestDelay>15</PostTestDelay></BoostTest></RunSettings>", Result = 15)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest /></RunSettings>", Result = 0)]
        public int ParsePostTestDelayOption(string settingsXml)
        {
            BoostTestAdapterSettings settings = ParseXml(settingsXml);
            return settings.PostTestDelay;
        }

        /// <summary>
        /// The 'CatchSystemErrors' option can be properly parsed
        /// 
        /// Test aims:
        ///     - Assert that: the 'CatchSystemErrors' option can be properly parsed
        /// </summary>
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><CatchSystemErrors>true</CatchSystemErrors></BoostTest></RunSettings>", Result = true)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><CatchSystemErrors>1</CatchSystemErrors></BoostTest></RunSettings>", Result = true)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><CatchSystemErrors>false</CatchSystemErrors></BoostTest></RunSettings>", Result = false)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><CatchSystemErrors>0</CatchSystemErrors></BoostTest></RunSettings>", Result = false)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest /></RunSettings>", Result = null)]
        public bool? ParseCatchSystemErrorsOption(string settingsXml)
        {
            BoostTestAdapterSettings settings = ParseXml(settingsXml);
            return settings.CatchSystemErrors;
        }
        
        /// <summary>
        /// The 'TestBatchStrategy' option can be properly parsed
        /// 
        /// Test aims:
        ///     - Assert that: The 'TestBatchStrategy' option can be properly parsed 
        /// </summary>
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><TestBatchStrategy>TestCase</TestBatchStrategy></BoostTest></RunSettings>", Result = Strategy.TestCase)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><TestBatchStrategy>TestSuite</TestBatchStrategy></BoostTest></RunSettings>", Result = Strategy.TestSuite)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><TestBatchStrategy>Source</TestBatchStrategy></BoostTest></RunSettings>", Result = Strategy.Source)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest><TestBatchStrategy>One</TestBatchStrategy></BoostTest></RunSettings>", Result = Strategy.One)]
        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\"?><RunSettings><BoostTest /></RunSettings>", Result = Strategy.TestCase)]
        public Strategy ParseTestBatchStrategy(string settingsXml)
        {
            BoostTestAdapterSettings settings = ParseXml(settingsXml);
            return settings.TestBatchStrategy;
        }

        #endregion Tests
    }
}
