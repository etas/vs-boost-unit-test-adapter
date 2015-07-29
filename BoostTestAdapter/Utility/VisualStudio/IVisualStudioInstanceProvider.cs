using VisualStudioAdapter;

namespace BoostTestAdapter.Utility.VisualStudio
{
    /// <summary>
    /// Abstract factory which provides IVisualStudio instances.
    /// </summary>
    public interface IVisualStudioInstanceProvider
    {
        /// <summary>
        /// Provides an IVisualStudio instance.
        /// </summary>
        /// <returns>An IVisualStudio instance or null if provisioning is not possible.</returns>
        IVisualStudio Instance { get; }
    }
}