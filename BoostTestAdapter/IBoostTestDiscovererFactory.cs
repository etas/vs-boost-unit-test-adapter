namespace BoostTestAdapter
{
    /// <summary>
    /// Abstract Factory which provides ITestDiscoverer instances.
    /// </summary>
    public interface IBoostTestDiscovererFactory
    {
        /// <summary>
        /// Returns an IBoostTestDiscoverer based on the provided identifier.
        /// </summary>
        /// <param name="identifier">A unique identifier able to distinguish different ITestDiscoverer types.</param>
        /// <param name="options">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>An IBoostTestDiscoverer instance or null if one cannot be provided.</returns>
        IBoostTestDiscoverer GetTestDiscoverer(string identifier, BoostTestDiscovererFactoryOptions options);
    }
}