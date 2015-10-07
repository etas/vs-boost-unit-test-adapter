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

            this.CommandLineArgs = new BoostTestRunnerCommandLineArgs();

            // Set default configuration values

            this.FailTestOnMemoryLeak = false;

            this.ConditionalInclusionsFilteringEnabled = true;

            this.LogLevel = LogLevel.TestSuite;

            this.ExternalTestRunner = null;

            this.CatchSystemErrors = true;

            this.DetectFloatingPointExceptions = false;
        }

        #region Properties

        #region Serialisable Fields

        [DefaultValue(-1)]
        public int TimeoutMilliseconds
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

        [DefaultValue(false)]
        public bool FailTestOnMemoryLeak { get; set; }

        [DefaultValue(true)]
        public bool ConditionalInclusionsFilteringEnabled { get; set; }

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

        #endregion Serialisable Fields

        [XmlIgnore]
        public BoostTestRunnerSettings TestRunnerSettings { get; private set; }

        [XmlIgnore]
        public BoostTestRunnerCommandLineArgs CommandLineArgs { get; private set; }

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