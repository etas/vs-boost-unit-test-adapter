// (C) Copyright ETAS 2015.
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
            DoNotUse,
            ForceUse
        }
        
        #region Tests

        /// <summary>
        /// Provisions internal and external TestDiscoverer instances ones based on the requested source and settings.
        /// 
        /// Test aims:
        ///     - Ensure that the proper ITestDiscoverer type is provided for the requested source.
        /// </summary>
        // Exe types
        [TestCase("test.exe", ListContentUse.DoNotUse, null, Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.exe", ListContentUse.Use, null, Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.exe", ListContentUse.ForceUse, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.DoNotUse, null, Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.Use, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.ForceUse, null, Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", ListContentUse.DoNotUse, ".dll", Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.exe", ListContentUse.Use, ".dll", Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.exe", ListContentUse.ForceUse, ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.DoNotUse, ".dll", Result = typeof(SourceCodeDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.Use, ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.ForceUse, ".dll", Result = typeof(ListContentDiscoverer))]
        [TestCase("test.exe", ListContentUse.DoNotUse, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.exe", ListContentUse.Use, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.exe", ListContentUse.ForceUse, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.Use, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.DoNotUse, ".exe", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.listcontent.exe", ListContentUse.ForceUse, ".exe", Result = typeof(ExternalDiscoverer))]
        // Dll types
        [TestCase("test.dll", ListContentUse.DoNotUse, null, Result = null)]
        [TestCase("test.dll", ListContentUse.Use, null, Result = null)]
        [TestCase("test.dll", ListContentUse.DoNotUse, ".dll", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.dll", ListContentUse.Use, ".dll", Result = typeof(ExternalDiscoverer))]
        [TestCase("test.dll", ListContentUse.DoNotUse, ".exe", Result = null)]
        [TestCase("test.dll", ListContentUse.Use, ".exe", Result = null)]
        // Invalid extension types
        [TestCase("test.txt", ListContentUse.DoNotUse, null, Result = null)]
        [TestCase("test.txt", ListContentUse.Use, null, Result = null)]
        [TestCase("test.txt", ListContentUse.DoNotUse, ".dll", Result = null)]
        [TestCase("test.txt", ListContentUse.Use, ".dll", Result = null)]
        [TestCase("test.txt", ListContentUse.DoNotUse, ".exe", Result = null)]
        [TestCase("test.txt", ListContentUse.Use, ".exe", Result = null)]
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
                UseListContent = (listContent == ListContentUse.Use) || (listContent == ListContentUse.ForceUse),
                ForceListContent = (listContent == ListContentUse.ForceUse)
            };

            IBoostTestDiscoverer discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

            return (discoverer == null) ? null : discoverer.GetType();
        }

        #endregion Tests
    }
}