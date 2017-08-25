// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
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
        // EXE types
        [TestCase("test.exe", null, null, Result = typeof(BoostTestRunner))]
        [TestCase("test.exe", "1.59", null, Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.exe", "1.62", null, Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.exe", "1.63", null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        [TestCase("test.exe", null, ".dll", Result = typeof(BoostTestRunner))]
        [TestCase("test.exe", "1.59", ".dll", Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.exe", "1.62", ".dll", Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.exe", "1.63", ".dll", Result = typeof(BoostTestRunnerCapabilityOverride))]

        [TestCase("test.exe", null, ".exe", Result = typeof(ExternalBoostTestRunner))]
        [TestCase("test.exe", "1.59", ".exe", Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.exe", "1.62", ".exe", Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.exe", "1.63", ".exe", Result = typeof(BoostTestRunnerCapabilityOverride))]
        
        // EXE types - .test.boostd.exe
        [TestCase("test.test.boostd.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        // EXE types - .test.boostd.exe (case-insensitive)
        [TestCase("test.TEST.BOOSTD.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        // EXE types - .test.boost.exe
        [TestCase("test.test.boost.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        // .EXE types - test.boostd.exe (case-insensitive)
        [TestCase("test.TEST.BOOST.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        // EXE types - .AcceptanceTest.boostd.exe
        [TestCase("test.AcceptanceTest.boostd.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        // EXE types - .Acceptancetest.boostd.exe (case-insensitive)
        [TestCase("test.Acceptancetest.boostd.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        // EXE types - .AcceptanceTest.boost.exe
        [TestCase("test.AcceptanceTest.boost.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]

        // EXE types - .Acceptancetest.boost.exe (case-insensitive)
        [TestCase("test.Acceptancetest.boost.exe", null, null, Result = typeof(BoostTestRunnerCapabilityOverride))]
        
        // DLL types 
        [TestCase("test.dll", null, null, Result = null)]
        [TestCase("test.dll", "1.59", null, Result = null)]
        [TestCase("test.dll", "1.62", null, Result = null)]
        [TestCase("test.dll", "1.63", null, Result = null)]

        [TestCase("test.dll", null, ".dll", Result = typeof(ExternalBoostTestRunner))]
        [TestCase("test.dll", "1.59", ".dll", Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.dll", "1.62", ".dll", Result = typeof(BoostTestRunnerCapabilityOverride))]
        [TestCase("test.dll", "1.63", ".dll", Result = typeof(BoostTestRunnerCapabilityOverride))]

        [TestCase("test.dll", null, ".exe", Result = null)]
        [TestCase("test.dll", "1.59", ".exe", Result = null)]
        [TestCase("test.dll", "1.62", ".exe", Result = null)]
        [TestCase("test.dll", "1.63", ".exe", Result = null)]

        // Invalid extension
        [TestCase("test.txt", null, null, Result = null)]
        [TestCase("test.txt", "1.59", null, Result = null)]
        [TestCase("test.txt", "1.62", null, Result = null)]
        [TestCase("test.txt", "1.63", null, Result = null)]

        [TestCase("test.txt", null, ".dll", Result = null)]
        [TestCase("test.txt", "1.59", ".dll", Result = null)]
        [TestCase("test.txt", "1.62", ".dll", Result = null)]
        [TestCase("test.txt", "1.63", ".dll", Result = null)]

        [TestCase("test.txt", null, ".exe", Result = null)]
        [TestCase("test.txt", "1.59", ".exe", Result = null)]
        [TestCase("test.txt", "1.62", ".exe", Result = null)]
        [TestCase("test.txt", "1.63", ".exe", Result = null)]
        public Type ExternalBoostTestRunnerProvisioning(string source, string boostTestVersion, string externalExtension)
        {
            var options = new BoostTestRunnerFactoryOptions()
            {
                ForcedBoostTestVersion = (string.IsNullOrEmpty(boostTestVersion)) ? null : Version.Parse(boostTestVersion)
            };

            if (externalExtension != null)
            {
                options.ExternalTestRunnerSettings = new ExternalBoostTestRunnerSettings
                {
                    ExtensionType = new Regex(externalExtension),
                    ExecutionCommandLine = new CommandLine()
                };
            }
            
            var runner = Factory.GetRunner(source, options);

            return runner?.GetType();
        }

        #endregion Tests
    }
}