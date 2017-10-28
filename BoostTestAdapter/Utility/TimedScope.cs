using System;
using System.Diagnostics;
using System.Globalization;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// RAII class which logs the time taken until
    /// disposed
    /// </summary>
    /// <remarks>Ideal for use with a 'using' scope</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Debug utility mimicking RAII")]
    public class TimedScope : IDisposable
    {
        /// <summary>
        /// Constructor which identifies a time scope via a string ID
        /// </summary>
        /// <param name="format">Format string for scope ID</param>
        /// <param name="args">Format arguments for format string</param>
        public TimedScope(string format, params object[] args)
        {
            ScopeId = string.Format(CultureInfo.InvariantCulture, format, args);

            Watch = new Stopwatch();
            Watch.Start();
        }

        /// <summary>
        /// Timed Scope ID
        /// </summary>
        public string ScopeId { get; private set; }

        /// <summary>
        /// Timed Scope StopWatch
        /// </summary>
        private Stopwatch Watch { get; set; }

        #region IDisposable

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Debug utility mimicking RAII")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Debug utility mimicking RAII")]
        public void Dispose()
        {
            var elapsed = Watch.ElapsedMilliseconds;
            Watch.Stop();

            Logger.Debug("Duration of \"{0}\": {1}ms", ScopeId, elapsed);
        }

        #endregion
    }
}
