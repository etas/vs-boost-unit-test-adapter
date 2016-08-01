// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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

    }
}