// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// Emulates a CPP source file. Copies an embedded resource as an OS temporary file using
    /// the embedded resource name as the file name.
    /// 
    /// Performs cleanup on calling Dispose.
    /// </summary>
    public class DummySourceFile : TemporaryFile
    {
        /// <summary>
        /// Default resource namespace
        /// </summary>
        private const string DefaultResourceNamespace = "BoostTestAdapterNunit.Resources.CppSources";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The embedded resource file name located in the default resource namespace</param>
        public DummySourceFile(string filename) :
            this(DefaultResourceNamespace, filename)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nameSpace">The embedded resource namespace</param>
        /// <param name="filename">The embedded resource file name located in nameSpace</param>
        public DummySourceFile(string nameSpace, string filename)
            : base(TestHelper.CopyEmbeddedResourceToTempDirectory(nameSpace, filename).Release())
        {
        }
    }
}
