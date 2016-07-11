// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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

            this.ExternalTestRunner = null;

            this.CatchSystemErrors = true;

            this.DetectFloatingPointExceptions = false;

            this.TestBatchStrategy = TestBatch.Strategy.TestCase;

            this.ForceListContent = false;

            this.WorkingDirectory = null;

            this.EnableStdOutRedirection = true;

            this.EnableStdErrRedirection = true;

            this.Filters = TestSourceFilter.Empty;

            this.RunDisabledTests = false;
        }

        #region Properties

        #region Serialisable Fields

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

        [DefaultValue(30000)]
        public int DiscoveryTimeoutMilliseconds { get; set; }

        [DefaultValue(false)]
        public bool FailTestOnMemoryLeak { get; set; }

        [DefaultValue(LogLevel.TestSuite)]
        public LogLevel LogLevel { get; set; }

        [DefaultValue(true)]
        public bool CatchSystemErrors
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

        public ExternalBoostTestRunnerSettings ExternalTestRunner { get; set; }

        [DefaultValue(TestBatch.Strategy.TestCase)]
        public TestBatch.Strategy TestBatchStrategy { get; set; }

        [DefaultValue(false)]
        public bool ForceListContent { get; set; }

        [DefaultValue(null)]
        public string WorkingDirectory { get; set; }
        
        [DefaultValue(true)]
        public bool EnableStdOutRedirection { get; set; }

        [DefaultValue(true)]
        public bool EnableStdErrRedirection { get; set; }

        public TestSourceFilter Filters { get; set; }

        #endregion Serialisable Fields

        [XmlIgnore]
        public BoostTestRunnerSettings TestRunnerSettings { get; private set; }

        [XmlIgnore]
        public BoostTestRunnerCommandLineArgs CommandLineArgs { get; private set; }

        [DefaultValue(false)]
        public bool RunDisabledTests { get; set; }

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