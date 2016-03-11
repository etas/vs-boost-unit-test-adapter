// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Reflection;

using BoostTestAdapter.Utility;

using NUnit.Framework;

namespace BoostTestAdapterNunit.Utility
{
    internal static class TestHelper
    {
        /// <summary>
        /// Loads in an embedded resource as a stream.
        /// </summary>
        /// <param name="path">The fully qualified path to the embedded resource</param>
        /// <returns>The embedded resource as a stream</returns>
        public static Stream LoadEmbeddedResource(string path)
        {
            // Reference: https://support.microsoft.com/en-us/kb/319292

            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(path);
        }

        /// <summary>
        /// Reads in an embedded resource as a string.
        /// </summary>
        /// <param name="path">The fully qualified path to the embedded resource</param>
        /// <returns>The whole content of the embedded resource as a string</returns>
        public static string ReadEmbeddedResource(string path)
        {
            using (Stream stream = LoadEmbeddedResource(path))
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Helper method so as to copy an embedded resource to the temporary directory
        /// </summary>
        /// <param name="nameSpace">namespace of the where the resource file is located at</param>
        /// <param name="resourceName">the filename of the embedded resource that needs to copied over</param>
        /// <returns>The temporary file of the successfully copied resource</returns>
        static public TemporaryFile CopyEmbeddedResourceToTempDirectory(string nameSpace, string resourceName)
        {
            string input = nameSpace + (nameSpace.EndsWith(".") ? "" : ".") + resourceName;
            string output = Path.Combine(Path.GetTempPath(), resourceName);

            return new TemporaryFile(CopyEmbeddedResourceToDirectory(input, output));
        }

        /// <summary>
        /// Helper method so as to copy an embedded resource to the path supplied
        /// </summary>
        /// <param name="embeddedResourePath">fully qualified path the where the resource file is located</param>
        /// <param name="outputPath">the path where the file need to be copied over to</param>
        /// <returns>The output path of the successfully copied resource</returns>
        static public string CopyEmbeddedResourceToDirectory(string embeddedResourePath, string outputPath)
        {
            string outputDirectoryPath = Path.GetDirectoryName(outputPath);
            Assert.That(Directory.Exists(outputDirectoryPath), Is.True, "The requested output directory ({0}) is invalid", outputDirectoryPath);
            
            using (Stream stream = LoadEmbeddedResource(embeddedResourePath))
            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                Assert.That(stream, Is.Not.Null, "Failed to load the requested embedded resource ({0}). Please check that the resource exists and the supplied embedded file namespace is correct", embeddedResourePath);
                stream.CopyTo(fileStream);
            }

            Assert.That(File.Exists(outputPath), Is.True, "Failed to copy embedded resource {0} to output {1}", embeddedResourePath, outputPath);

            return outputPath;
        }
    }
}
