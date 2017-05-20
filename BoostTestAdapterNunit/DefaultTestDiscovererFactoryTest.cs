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
            this.DiscovererFactory = new BoostTestDiscovererFactory(this.RunnerFactory);
        }

        #endregion Test Setup/Teardown

        #region Test Data

        private IBoostTestRunnerFactory RunnerFactory { get; set; }

        private BoostTestDiscovererFactory DiscovererFactory { get; set; }

        #endregion Test Data

        internal enum ListContentUse
        {
            Use,
            ForceUse,
            Default = Use
        }

        #region Tests

        /// <summary>
        /// Provisions internal and external TestDiscoverer instances ones based on the requested source and settings.
        /// 
        /// Test aims:
        ///     - Ensure that the proper ITestDiscoverer type is provided for the requested source.
        /// </summary>
        // Exe types
        [TestCase("test.exe", ListContentUse.Use, null, Result = null)]
        [TestCase("test.exe", ListContentUse.ForceUse, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.Use, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.ForceUse, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", ListContentUse.Use, ".dll", Result = null)]
        [TestCase("test.exe", ListContentUse.ForceUse, ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.Use, ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.ForceUse, ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", ListContentUse.Use, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.exe", ListContentUse.ForceUse, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.Use, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.ForceUse, ".exe", Result = typeof(ExternalDiscoverer))]
        // .test.boostd.exe
        [TestCase("test.test.boostd.exe", ListContentUse.Use, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.test.boostd.exe", ListContentUse.ForceUse, null, Result = typeof(ListContentDiscoverer))]
        // Dll types
        [TestCase("test.dll", ListContentUse.Use, null, Result = null)]
        [TestCase("test.dll", ListContentUse.Use, ".dll", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.dll", ListContentUse.Use, ".exe", Result = null)]
        [TestCase("test.dll", ListContentUse.ForceUse, null, Result = null)]
        [TestCase("test.dll", ListContentUse.ForceUse, ".dll", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.dll", ListContentUse.ForceUse, ".exe", Result = null)]
        // Invalid extension types
        [TestCase("test.txt", ListContentUse.Use, null, Result = null)]
        [TestCase("test.txt", ListContentUse.Use, ".dll", Result = null)]
        [TestCase("test.txt", ListContentUse.Use, ".exe", Result = null)]
        [TestCase("test.txt", ListContentUse.ForceUse, null, Result = null)]
        [TestCase("test.txt", ListContentUse.ForceUse, ".dll", Result = null)]
        [TestCase("test.txt", ListContentUse.ForceUse, ".exe", Result = null)]
        public Type TestDiscovererProvisioning(string source, ListContentUse listContent, string externalExtension)
        {
            ExternalBoostTestRunnerSettings externalSettings = null;
            
            if (!string.IsNullOrEmpty(externalExtension))
            {
                externalSettings = new ExternalBoostTestRunnerSettings { ExtensionType = new Regex(externalExtension) };
            }

            BoostTestAdapterSettings settings = new BoostTestAdapterSettings()
            {
                ExternalTestRunner = externalSettings,
                ForceListContent = (listContent == ListContentUse.ForceUse)
            };

            IBoostTestDiscoverer discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

            return (discoverer == null) ? null : discoverer.GetType();
        }

        #endregion Tests
    }
}