// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter.Utility.VisualStudio;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// Builds and emulates a Visual Studio solution with a single test project.
    /// Copies resource files to their intended location for later reference.
    /// </summary>
    public class DummySolution
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The fully qualified path of a test source</param>
        /// <param name="sourceFileName">The embedded resource file name which is to be considered as the sole source file for this test project</param>
        public DummySolution(string source, string sourceFileName) :
            this(source, new string[] { sourceFileName })
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The fully qualified path of a test source</param>
        /// <param name="sourceFileNames">An enumeration of embedded resource file names which are to be considered as source files for this test project</param>
        public DummySolution(string source, IEnumerable<string> sourceFileNames)
        {
            this.Source = source;
            this.SourceFiles = sourceFileNames;

        }


        /// <summary>
        /// The test source
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// The test code source files
        /// </summary>
        public IEnumerable<string> SourceFiles { get; private set; }

        /// <summary>
        /// The IVisualStudio instance provider for this DummySolution
        /// </summary>
        public IVisualStudioInstanceProvider Provider { get; private set; }

     }
}
