using System.IO;
using System.Reflection;
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
        /// Helper method so as to copy an embedded resource to the path supplied
        /// </summary>
        /// <param name="nameSpace">namespace of the where the resource file is located at</param>
        /// <param name="resourceName">the filename of the embedded resource that needs to copied over</param>
        /// <param name="outputDirectoryPath">the path where the file need to be copied over to</param>
        /// <returns>The output path of the successfully copied resource</returns>
        static public string CopyEmbeddedResourceToDirectory(string nameSpace, string resourceName, string outputDirectoryPath)
        {
            if (!Directory.Exists(outputDirectoryPath))
            {
                Assert.Fail("The requested output directory is invalid");
            }

            string input = nameSpace + (nameSpace.EndsWith(".") ? "" : ".") + resourceName;
            string output = Path.Combine(outputDirectoryPath, resourceName);

            using (Stream stream = LoadEmbeddedResource(input))
            using (FileStream fileStream = new FileStream(output, FileMode.Create, FileAccess.Write))
            {
                if (stream == null)
                {
                    Assert.Fail("Failed to load the requested embedded resource. Please check that the resource exists and the supplied embedded file namespace is correct");
                }
                stream.CopyTo(fileStream);
            }

            if (!File.Exists(output))
            {
                Assert.Fail("Failed to copy embedded resource " + nameSpace + resourceName + " to output directory " + outputDirectoryPath);
            }

            return output;
        }
    }
}
