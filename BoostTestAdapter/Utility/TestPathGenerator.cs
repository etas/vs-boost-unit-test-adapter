// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Threading;
using System.Diagnostics;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Utility class which generates paths for test resources
    /// </summary>
    public static class TestPathGenerator
    {
        /// <summary>
        /// Generates a temporary file path based on the test source. Guaranteed to be unique across multiple executions of the same test source.
        /// </summary>
        /// <param name="source">The test source to execute</param>
        /// <returns>A file name suitable for generating log, report, stdout and stderr output paths</returns>
        public static string Generate(string source)
        {
            return Generate(source, string.Empty);
        }

        /// <summary>
        /// Generates a temporary file path based on the test source. Guaranteed to be unique across multiple executions of the same test source.
        /// </summary>
        /// <param name="source">The test source to execute</param>
        /// <param name="extension">The file extension</param>
        /// <returns>A file name suitable for generating log, report, stdout and stderr output paths</returns>
        public static string Generate(string source, string extension)
        {
            return Path.Combine(Path.GetTempPath(), GenerateFileName(source, extension));
        }

        /// <summary>
        /// Generates a temporary file name based on the test source. Guaranteed to be unique across multiple executions of the same test source.
        /// </summary>
        /// <param name="source">The test source to execute</param>
        /// <returns>A file name suitable for generating log, report, stdout and stderr output paths</returns>
        public static string GenerateFileName(string source)
        {
            return GenerateFileName(source, string.Empty);
        }

        /// <summary>
        /// Generates a temporary file name based on the test source. Guaranteed to be unique across multiple executions of the same test source.
        /// </summary>
        /// <param name="source">The test source to execute</param>
        /// <param name="extension">The file extension</param>
        /// <returns>A file name suitable for generating log, report, stdout and stderr output paths</returns>
        public static string GenerateFileName(string source, string extension)
        {
            return Sanitize(Path.GetFileName(source)) + '.' + Process.GetCurrentProcess().Id + '.' + Thread.CurrentThread.ManagedThreadId + Sanitize(extension);
        }

        /// <summary>
        /// Sanitizes a file name component suitable for Boost Test command line argument values
        /// </summary>
        /// <param name="value">The file name component to sanitize.</param>
        /// <returns>The sanitized file name component.</returns>
        private static string Sanitize(string value)
        {
            return value.Replace(' ', '_');
        }
    }
}
