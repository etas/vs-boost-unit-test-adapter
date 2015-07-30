namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// IBoostTestRunner implementation. Executes stand-alone
    /// (i.e. test runner included within '.exe') Boost Tests.
    /// </summary>
    public class BoostTestRunner : BoostTestRunnerBase
    {
        public BoostTestRunner(string exe) :
            base(exe)
        {
        }
    }
}