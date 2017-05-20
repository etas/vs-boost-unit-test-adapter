// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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