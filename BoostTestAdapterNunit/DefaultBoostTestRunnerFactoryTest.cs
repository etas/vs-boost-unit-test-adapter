// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Text.RegularExpressions;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class DefaultBoostTestRunnerFactoryTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.Factory = new DefaultBoostTestRunnerFactory();
        }
        
        #endregion Test Setup/Teardown

        #region Test Data

        private DefaultBoostTestRunnerFactory Factory { get; set; }

        #endregion Test Data

        #region Tests
        
        /// <summary>
        /// Provisions internal and external IBoostTestRunner instances based on the requested source and settings.
        /// 
        /// Test aims:
        ///     - Ensure that the proper IBoostTestRunner type is provided for the requested source.
        /// </summary>
        // Exe types
        [TestCase("test.exe", null, typeof(BoostTestRunner))]
        [TestCase("test.exe", ".dll", typeof(BoostTestRunner))]
        [TestCase("test.exe", ".exe", typeof(ExternalBoostTestRunner))]
        // Dll types
        [TestCase("test.dll", null, null)]
        [TestCase("test.dll", ".dll", typeof(ExternalBoostTestRunner))]
        [TestCase("test.dll", ".exe", null)]
        // Invalid extension types
        [TestCase("test.txt", null, null)]
        [TestCase("test.txt", ".dll", null)]
        [TestCase("test.txt", ".exe", null)]
        public void ExternalBoostTestRunnerProvisioning(string source, string externalExtension, Type type)
        {
            BoostTestRunnerFactoryOptions options = new BoostTestRunnerFactoryOptions();
            if (externalExtension != null)
            {
                options.ExternalTestRunnerSettings = new ExternalBoostTestRunnerSettings
                {
                    ExtensionType = new Regex(externalExtension),
                    DiscoveryCommandLine = new CommandLine(),
                    ExecutionCommandLine = new CommandLine()
                };
            }
            
            IBoostTestRunner runner = this.Factory.GetRunner(source, options);

            if (runner == null)
            {
                Assert.That(type, Is.Null);
            }
            else
            {
                Assert.That(runner, Is.AssignableTo(type));
            }
        }

        #endregion Tests
    }
}