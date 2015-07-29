using System;
using BoostTestAdapter;
using BoostTestAdapter.Settings;
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
            this.Factory = new DefaultBoostTestDiscovererFactory();
        }

        #endregion Test Setup/Teardown

        #region Test Data

        private DefaultBoostTestDiscovererFactory Factory { get; set; }

        #endregion Test Data

        #region Tests

        /// <summary>
        /// Provisions internal and external TestDiscoverer instances ones based on the requested source and settings.
        /// 
        /// Test aims:
        ///     - Ensure that the proper ITestDiscoverer type is provided for the requested source.
        /// </summary>
        // Exe types
        [TestCase("test.exe", null, Result = typeof(BoostTestExeDiscoverer))]
        [TestCase("test.exe", ".dll", Result = typeof(BoostTestExeDiscoverer))]
        [TestCase("test.exe", ".exe", Result = typeof(ExternalBoostTestDiscoverer))]
        // Dll types
        [TestCase("test.dll", null, Result = null)]
        [TestCase("test.dll", ".dll", Result = typeof(ExternalBoostTestDiscoverer))]
        [TestCase("test.dll", ".exe", Result = null)]
        // Invalid extension types
        [TestCase("test.txt", null, Result = null)]
        [TestCase("test.txt", ".dll", Result = null)]
        [TestCase("test.txt", ".exe", Result = null)]
        public Type TestDiscovererProvisioning(string source, string externalExtension)
        {
            ExternalBoostTestRunnerSettings settings = null;

            if (!string.IsNullOrEmpty(externalExtension))
            {
                settings = new ExternalBoostTestRunnerSettings { ExtensionType = externalExtension };
            }

            BoostTestDiscovererFactoryOptions options = new BoostTestDiscovererFactoryOptions
            {
                ExternalTestRunnerSettings = settings
            };

            IBoostTestDiscoverer discoverer = this.Factory.GetTestDiscoverer(source, options);

            return (discoverer == null) ? null : discoverer.GetType();
        }

        #endregion Tests
    }
}