﻿// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace VisualStudioAdapter
{
    /// <summary>
    /// Abstraction for a Visual Studio Project Configuration
    /// </summary>
    public interface IProjectConfiguration
    {
        /// <summary>
        /// Determines the fully-qualified path of the project's primary output
        /// </summary>
        string PrimaryOutput { get; }

        /// <summary>
        /// C++ compiler options. If this project is not a C++ project, this property returns null.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
        IVCppCompilerOptions CppCompilerOptions { get; }
    }
}