// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Aggregates a C++ source file path and its content.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
    public class CppSourceFile
    {
        /// <summary>
        /// Source code content of the C++ source file.
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// C++ source file path.
        /// </summary>
        public string FileName { get; set; }
    }
}