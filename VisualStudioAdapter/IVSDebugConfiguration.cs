// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace VisualStudioAdapter
{
    /// <summary>
    /// Abstracts a Visual Studio Debug Configuration
    /// </summary>
    public interface IVSDebugConfiguration
    {
        /// <summary>
        /// Retrieves Working Directory
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// Retrieves Environment
        /// </summary>
        string Environment { get; }
    }    
}
