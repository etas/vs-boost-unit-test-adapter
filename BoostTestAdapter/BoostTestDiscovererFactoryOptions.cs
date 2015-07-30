using BoostTestAdapter.Settings;

namespace BoostTestAdapter
{
    /// <summary>
    /// Options for Boost Test discoverer provisioning
    /// </summary>
    public class BoostTestDiscovererFactoryOptions
    {
        public ExternalBoostTestRunnerSettings ExternalTestRunnerSettings { get; set; }
    }
}
