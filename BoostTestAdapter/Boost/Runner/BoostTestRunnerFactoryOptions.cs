using BoostTestAdapter.Settings;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Aggregates all options for BoostTestRunnerFactory
    /// </summary>
    public class BoostTestRunnerFactoryOptions
    {
        public ExternalBoostTestRunnerSettings ExternalTestRunnerSettings { get; set; }
    }
}