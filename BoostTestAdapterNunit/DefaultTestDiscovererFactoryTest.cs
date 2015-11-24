// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using BoostTestAdapter;
using BoostTestAdapter.Settings;
using NUnit.Framework;
using BoostTestAdapter.Discoverers;
using BoostTestAdapterNunit.Fakes;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class DefaultTestDiscovererFactoryTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            IListContentHelper helper = new StubListContentHelper("test.exe");
            this.Factory = new BoostTestDiscovererFactory(helper);
        }

        #endregion Test Setup/Teardown

        #region Test Data

        private BoostTestDiscovererFactory Factory { get; set; }

        #endregion Test Data

        #region Tests

        /// <summary>
        /// Provisions internal and external TestDiscoverer instances ones based on the requested source and settings.
        /// 
        /// Test aims:
        ///     - Ensure that the proper ITestDiscoverer type is provided for the requested source.
        /// </summary>
        // Exe types
        [TestCase("test.exe", null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", ".exe", Result = typeof(ExternalDiscoverer))]
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
                externalSettings = new ExternalBoostTestRunnerSettings { ExtensionType = externalExtension };
            }

            BoostTestAdapterSettings settings = new BoostTestAdapterSettings()
            {
                ExternalTestRunner = externalSettings
            };

            IBoostTestDiscoverer discoverer = this.Factory.GetDiscoverer(source, settings);

            return (discoverer == null) ? null : discoverer.GetType();
        }

        #endregion Tests
    }
}