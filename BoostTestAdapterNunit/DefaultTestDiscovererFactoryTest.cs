// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Text.RegularExpressions;

using BoostTestAdapter;
using BoostTestAdapter.Settings;

using BoostTestAdapter.Discoverers;
using BoostTestAdapter.Boost.Runner;

using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;

using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class DefaultTestDiscovererFactoryTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.RunnerFactory = new StubBoostTestRunnerFactory(new[] { "test.listcontent.exe" });
            this.DiscovererFactory = new BoostTestDiscovererFactory(this.RunnerFactory, DummyVSProvider.Default);
        }

        #endregion Test Setup/Teardown

        #region Test Data

        private IBoostTestRunnerFactory RunnerFactory { get; set; }

        private BoostTestDiscovererFactory DiscovererFactory { get; set; }

        #endregion Test Data

        #region Tests

        /// <summary>
        /// Provisions internal and external TestDiscoverer instances ones based on the requested source and settings.
        /// 
        /// Test aims:
        ///     - Ensure that the proper ITestDiscoverer type is provided for the requested source.
        /// </summary>
        // Exe types - No '--list_content' support
        [TestCase("test.exe", null, Result = null)]
        [TestCase("test.exe", ".dll", Result = null)]
        [TestCase("test.exe", ".exe", Result = typeof(ExternalDiscoverer))]
        // Exe types - '--list_content' support
        [TestCase("test.listcontent.exe", null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ".exe", Result = typeof(ExternalDiscoverer))]
        // Dll types
        [TestCase("test.dll", null, Result = null)]
        [TestCase("test.dll", ".dll", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.dll", ".exe", Result = null)]
        // Invalid extension types
        [TestCase("test.txt", null, Result = null)]
        [TestCase("test.txt", ".dll", Result = null)]
        [TestCase("test.txt", ".exe", Result = null)]
        public Type TestDiscovererProvisioning(string source, string externalExtension)
        {
            ExternalBoostTestRunnerSettings externalSettings = null;
            
            if (!string.IsNullOrEmpty(externalExtension))
            {
                externalSettings = new ExternalBoostTestRunnerSettings { ExtensionType = new Regex(externalExtension) };
            }

            BoostTestAdapterSettings settings = new BoostTestAdapterSettings()
            {
                ExternalTestRunner = externalSettings
            };

            IBoostTestDiscoverer discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

            return (discoverer == null) ? null : discoverer.GetType();
        }

        #endregion Tests
    }
}