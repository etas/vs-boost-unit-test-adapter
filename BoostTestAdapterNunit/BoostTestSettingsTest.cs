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
            Assert.That(settings.DiscoveryTimeoutMilliseconds, Is.EqualTo(30000));
            Assert.That(settings.FailTestOnMemoryLeak, Is.False);
            Assert.That(settings.LogLevel, Is.EqualTo(LogLevel.TestSuite));
            Assert.That(settings.ExternalTestRunner, Is.Null);
            Assert.That(settings.DetectFloatingPointExceptions, Is.False);
            Assert.That(settings.CatchSystemErrors, Is.True);
            Assert.That(settings.TestBatchStrategy, Is.EqualTo(Strategy.TestCase));
            Assert.That(settings.ForceListContent, Is.False);
            Assert.That(settings.WorkingDirectory, Is.Null);
            Assert.That(settings.EnableStdOutRedirection, Is.True);
            Assert.That(settings.EnableStdErrRedirection, Is.True);
            Assert.That(settings.Filters, Is.EqualTo(TestSourceFilter.Empty));
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

            Assert.That(settings.Filters, Is.Not.Null);
            Assert.That(settings.Filters.Include, Is.Not.Empty);
            Assert.That(settings.Filters.Include, Is.EquivalentTo(new[] { "mytest.exe$" }));

            Assert.That(settings.Filters.Exclude, Is.Not.Empty);
            Assert.That(settings.Filters.Exclude, Is.EquivalentTo(new[] { "test.exe$" }));
        }

        
   

        #endregion Tests
    }
}
