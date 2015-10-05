// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace VisualStudioAdapter
{
    /// <summary>
    /// Abstraction for Visual Studio C++ Compiler Options
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
    public interface IVCppCompilerOptions
    {
        /// <summary>
        /// Lists all Preprocessor defines currently configured. This includes all properties, even those configured within property sheets.
        /// </summary>
        Defines PreprocessorDefinitions { get; }
    }
}