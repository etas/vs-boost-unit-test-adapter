using System.IO;
using BoostTestAdapter.Settings;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Default implementation for IBoostTestRunnerFactory.
    /// </summary>
    public class DefaultBoostTestRunnerFactory : IBoostTestRunnerFactory
    {
        #region IBoostTestRunnerFactory

        /// <summary>
        /// Based on the provided file name, returns a suitable IBoostTestRunner
        /// instance or null if none are available.
        /// </summary>
        /// <param name="identifier">The identifier which is to be executed via the IBoostTestRunner.</param>
        /// <param name="options">test runner settings</param>
        /// <returns>A suitable IBoostTestRunner instance or null if none are available.</returns>
        public IBoostTestRunner GetRunner(string identifier, BoostTestRunnerFactoryOptions options)
        {
            IBoostTestRunner runner = null;

            if ((options != null) && (options.ExternalTestRunnerSettings != null))
            {
                // Provision an external test runner
                runner = GetExternalTestRunner(identifier, options.ExternalTestRunnerSettings);
            }

            // Provision a default internal runner
            if (runner == null)
            {
                runner = GetInternalTestRunner(identifier);
            }

            return runner;
        }

        private static IBoostTestRunner GetInternalTestRunner(string source)
        {
            switch (Path.GetExtension(source))
            {
                case ".exe": return new BoostTestRunner(source);
            }

            return null;
        }

        private static IBoostTestRunner GetExternalTestRunner(string source, ExternalBoostTestRunnerSettings settings)
        {
            Utility.Code.Require(settings, "settings");

            if (settings.ExtensionType == Path.GetExtension(source))
            {
                return new ExternalBoostTestRunner(source, settings);
            }

            return null;
        }

        #endregion IBoostTestRunnerFactory
    }
}