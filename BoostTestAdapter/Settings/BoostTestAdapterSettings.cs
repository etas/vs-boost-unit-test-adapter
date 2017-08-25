// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using BoostTestAdapter.Boost.Runner;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace BoostTestAdapter.Settings
{
    /// <summary>
    /// Settings relating to the Boost Test Visual Studio Adapter.
    /// </summary>
    /// <remarks>
    /// Distinguish between BoostTestAdapterSettings and BoostTestRunnerSettings.
    /// BoostTestAdapterSettings aggregate both adapter specific settings and
    /// BoostTestRunner settings as required.
    /// </remarks>
    [XmlRoot(XmlRootName)]
    public class BoostTestAdapterSettings : TestRunSettings
    {
        public const string XmlRootName = "BoostTest";

        public BoostTestAdapterSettings() :
            base(XmlRootName)
        {
            this.TestRunnerSettings = new BoostTestRunnerSettings();

            this.DiscoveryTimeoutMilliseconds = 30000;

            this.CommandLineArgs = new BoostTestRunnerCommandLineArgs();
            
            this.FailTestOnMemoryLeak = false;

            this.LogLevel = LogLevel.TestSuite;
            
            this.CatchSystemErrors = null;

            this.DetectFloatingPointExceptions = false;

            this.TestBatchStrategy = TestBatch.Strategy.TestCase;
            
            this.WorkingDirectory = null;

            this.EnableStdOutRedirection = true;

            this.EnableStdErrRedirection = true;

            this.Filters = TestSourceFilter.Empty;

            this.RunDisabledTests = false;

            this.TestRunnerFactoryOptions = new BoostTestRunnerFactoryOptions();

            this.PostTestDelay = 0;

            this.ForceBoostVersion = null;
        }

        #region Properties

        #region Serialisable Fields

        /// <summary>
        /// Specifies the duration in milliseconds test process are allowed to execute until forcibly terminated. Use '-1' to allow tests to execute indefinitely.
        /// </summary>
        [DefaultValue(-1)]
        public int ExecutionTimeoutMilliseconds
        {
            get
            {
                return this.TestRunnerSettings.Timeout;
            }

            set
            {
                this.TestRunnerSettings.Timeout = value;
            }
        }

        /// <summary>
        /// Specifies the duration in milliseconds test process are allowed to execute in order to have their list_content output analyzed until forcibly terminated.
        /// </summary>
        [DefaultValue(30000)]
        public int DiscoveryTimeoutMilliseconds { get; set; }

        /// <summary>
        /// Set to 'true'|'1' to fail tests in case memory leaks are detected.
        /// </summary>
        [DefaultValue(false)]
        public bool FailTestOnMemoryLeak { get; set; }

        /// <summary>
        /// Specify Boost Test log verbosity.
        /// </summary>
        [DefaultValue(LogLevel.TestSuite)]
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Set to 'true'|'1' to enable Boost Test's catch_system_errors.
        /// </summary>
        [DefaultValue(null)]
        public bool? CatchSystemErrors
        {
            get
            {
                return this.CommandLineArgs.CatchSystemErrors;
            }

            set
            {
                this.CommandLineArgs.CatchSystemErrors = value;
            }
        }

        /// <summary>
        /// Set to 'true'|'1' to enable Boost Test's detect_fp_exceptions.
        /// </summary>
        [DefaultValue(false)]
        public bool DetectFloatingPointExceptions
        {
            get
            {
                return this.CommandLineArgs.DetectFPExceptions;
            }

            set
            {
                this.CommandLineArgs.DetectFPExceptions = value;
            }
        }

        /// <summary>
        /// Describes the discovery and test execution mechanisms for Boost Test executables using a custom runner implementation.
        /// </summary>
        public ExternalBoostTestRunnerSettings ExternalTestRunner
        {
            get
            {
                return this.TestRunnerFactoryOptions.ExternalTestRunnerSettings;
            }

            set
            {
                this.TestRunnerFactoryOptions.ExternalTestRunnerSettings = value;
            }
        }

        /// <summary>
        /// Determines how tests should be grouped for execution.
        /// </summary>
        [DefaultValue(TestBatch.Strategy.TestCase)]
        public TestBatch.Strategy TestBatchStrategy { get; set; }

        /// <summary>
        /// Forces the use of 'list_content=DOT' even if the test module is not recognized as a safe module.
        /// </summary>
        /// <remarks>Deprecated. This configuration element is superseded by '<ForceBoostVersion>'</remarks>
        [DefaultValue(false), Obsolete("This configuration element is superseded by '<ForceBoostVersion>'")]
        public bool ForceListContent
        {
            get
            {
                return (TestRunnerFactoryOptions.ForcedBoostTestVersion != null) &&
                    (TestRunnerFactoryOptions.ForcedBoostTestVersion >= DefaultBoostTestRunnerFactory.Boost159);
            }

            set
            {
                if (value && (TestRunnerFactoryOptions.ForcedBoostTestVersion == null))
                {
                    TestRunnerFactoryOptions.ForcedBoostTestVersion = DefaultBoostTestRunnerFactory.Boost159;
                }
            }
        }

        /// <summary>
        /// Determines the working directory which is to be used during the discovery/execution of the test module. If the test module is executed within a Visual Studio test adapter session, the Working Directory defined in the 'Debug' property sheet configuration overrides this value.
        /// </summary>
        [DefaultValue(null)]
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Enables/Disables standard output redirection. Note that if disabled, memory leak detection is implicitly disabled.
        /// </summary>
        [DefaultValue(true)]
        public bool EnableStdOutRedirection { get; set; }

        /// <summary>
        /// Enables/Disables standard error redirection.
        /// </summary>
        [DefaultValue(true)]
        public bool EnableStdErrRedirection { get; set; }

        /// <summary>
        /// Define a series of regular expression patterns which determine which test modules should be taken into consideration for discovery/execution.
        /// </summary>
        public TestSourceFilter Filters { get; set; }

        /// <summary>
        /// Enables/Disables the Boost 1.62 workaround. Allows the use of the test adapter with Boost Test released with Boost 1.62.
        /// </summary>
        [DefaultValue(false)]
        public bool UseBoost162Workaround
        {
            get
            {
                return this.TestRunnerFactoryOptions.UseBoost162Workaround;
            }

            set
            {
                this.TestRunnerFactoryOptions.UseBoost162Workaround = value;
            }
        }

        /// <summary>
        /// Set to 'true'|'1' to force explicitly disabled tests to be run with any other enabled tests if "Run all..." is pressed. By default, only enabled tests are run.
        /// </summary>
        [DefaultValue(false)]
        public bool RunDisabledTests { get; set; }

        /// <summary>
        /// Determines a delay represented in milliseconds which will be forced after the execution of each test batch.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PostTest")]
        [DefaultValue(0)]
        public int PostTestDelay { get; set; }

        /// <summary>
        /// Assumes/Forces the use of a specific Boost version even if the test module is not recognized as a safe module.
        /// </summary>
        /// <remarks>Assumes Boost.Test capabilities from the specified version. This configuration element supersedes '<ForceListContent>'</remarks>
        [DefaultValue(null)]
        public string ForceBoostVersion
        {
            get
            {
                return (TestRunnerFactoryOptions.ForcedBoostTestVersion == null) ? string.Empty : TestRunnerFactoryOptions.ForcedBoostTestVersion.ToString();
            }

            set
            {
                TestRunnerFactoryOptions.ForcedBoostTestVersion = (string.IsNullOrEmpty(value) ? null : Version.Parse(value));
            }
        }

        #endregion Serialisable Fields

        /// <summary>
        /// Internal Test Runner Settings. Serializable properties implicitly populate this instance.
        /// </summary>
        [XmlIgnore]
        public BoostTestRunnerSettings TestRunnerSettings { get; private set; }

        /// <summary>
        /// Internal Command Line Arguments. Serializable properties implicitly populate this instance.
        /// </summary>
        [XmlIgnore]
        public BoostTestRunnerCommandLineArgs CommandLineArgs { get; private set; }

        /// <summary>
        /// Internal Test Runner Factory Options. Serializable properties implicitly populate this instance.
        /// </summary>
        [XmlIgnore]
        public BoostTestRunnerFactoryOptions TestRunnerFactoryOptions { get; private set; }

        #endregion Properties

        #region TestRunSettings

        public override XmlElement ToXml()
        {
            XmlDocument doc = new XmlDocument();

            using (XmlWriter writer = doc.CreateNavigator().AppendChild())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BoostTestAdapterSettings));
                serializer.Serialize(writer, this);
            }

            // Remove any namespace related attributes
            doc.DocumentElement.RemoveAllAttributes();

            return doc.DocumentElement;
        }

        #endregion TestRunSettings
    }
}