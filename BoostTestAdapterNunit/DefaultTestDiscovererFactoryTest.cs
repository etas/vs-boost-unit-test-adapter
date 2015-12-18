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
        // Exe types (SourceCodeDiscovery)
        [TestCase("test.exe", false, null, Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.exe", true, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", false, ".dll", Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.exe", true, ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", false, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.exe", true, ".exe", Result = typeof(ExternalDiscoverer))]
        // Dll types
        [TestCase("test.dll", false, null, Result = null)]
        [TestCase("test.dll", true, null, Result = null)]
        [TestCase("test.dll", false, ".dll", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.dll", true, ".dll", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.dll", false, ".exe", Result = null)]
        [TestCase("test.dll", true, ".exe", Result = null)]
        // Invalid extension types
        [TestCase("test.txt", false, null, Result = null)]
        [TestCase("test.txt", true, null, Result = null)]
        [TestCase("test.txt", false, ".dll", Result = null)]
        [TestCase("test.txt", true, ".dll", Result = null)]
        [TestCase("test.txt", false, ".exe", Result = null)]
        [TestCase("test.txt", true, ".exe", Result = null)]
        public Type TestDiscovererProvisioning(string source, bool useListContent, string externalExtension)
        {
            ExternalBoostTestRunnerSettings externalSettings = null;
            
            if (!string.IsNullOrEmpty(externalExtension))
            {
                externalSettings = new ExternalBoostTestRunnerSettings { ExtensionType = externalExtension };
            }

            BoostTestAdapterSettings settings = new BoostTestAdapterSettings()
            {
                ExternalTestRunner = externalSettings,
                UseListContent = useListContent
            };

            IBoostTestDiscoverer discoverer = this.Factory.GetDiscoverer(source, settings);

            return (discoverer == null) ? null : discoverer.GetType();
        }

        #endregion Tests
    }
}