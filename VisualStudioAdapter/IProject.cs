using System.Collections.Generic;

namespace VisualStudioAdapter
{
    /// <summary>
    /// Abstracts a Visual Studio Project
    /// </summary>
    public interface IProject
    {
        /// <summary>
        /// Project Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The projects active configuration. This is determined by the currently selected platform type and configuration.
        /// </summary>
        IProjectConfiguration ActiveConfiguration { get; }

        /// <summary>
        /// Enumeration of source files to be parsed.
        /// </summary>
        IEnumerable<string> SourceFiles { get; }
    }
}