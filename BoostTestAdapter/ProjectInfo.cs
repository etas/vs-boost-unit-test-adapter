// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using VisualStudioAdapter;

namespace BoostTestAdapter
{
    /// <summary>
    /// Aggregates necessary project information for EXE Boost test discovery.
    /// </summary>
    public class ProjectInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectExe">The EXE test source path</param>
        public ProjectInfo(string projectExe)
        {
            ProjectExe = projectExe;
            CppSourceFiles = new List<string>();
        }

        /// <summary>
        /// Preprocessor definitions in use by the test source
        /// </summary>
        public Defines DefinesHandler { get; set; }

        /// <summary>
        /// Collection of C++ source files related to the test source
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
        public IList<string> CppSourceFiles { get; private set; }

        /// <summary>
        /// Boost Test EXE source path
        /// </summary>
        public string ProjectExe { get; private set; }
    }
}