// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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