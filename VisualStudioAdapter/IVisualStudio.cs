namespace VisualStudioAdapter
{
    /// <summary>
    /// Abstracts a Visual Studio runnning instance.
    /// </summary>
    public interface IVisualStudio
    {
        /// <summary>
        /// Visual Studio version. Identifies the incremental version e.g. VS2012 -> 11, VS2013 -> 12
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Currently loaded solution.
        /// </summary>
        ISolution Solution { get; }
    }
}