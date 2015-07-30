using System.Collections.Generic;

namespace VisualStudioAdapter
{
    /// <summary>
    /// Abstraction for a Visual Studio solution.
    /// </summary>
    public interface ISolution
    {
        /// <summary>
        /// Solution Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Enumeration of all child projects
        /// </summary>
        IEnumerable<IProject> Projects { get; }
    }
}