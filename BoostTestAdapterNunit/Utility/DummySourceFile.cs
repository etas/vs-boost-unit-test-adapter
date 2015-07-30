using System;
using System.IO;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// Emulates a CPP source file. Copies an embedded resource as an OS temporary file using
    /// the embedded resource name as the file name.
    /// 
    /// Performs cleanup on calling Dispose.
    /// </summary>
    public class DummySourceFile : IDisposable
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
        {
            this.TempSourcePath = TestHelper.CopyEmbeddedResourceToDirectory(nameSpace, filename, Path.GetTempPath());
        }

        /// <summary>
        /// The temporary file path of the copied embedded resource
        /// </summary>
        public string TempSourcePath { get; private set; }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!string.IsNullOrEmpty(this.TempSourcePath) && File.Exists(this.TempSourcePath))
                {
                    File.Delete(this.TempSourcePath);
                }
            }

            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion IDisposable
    }
}
