using System;
using System.IO;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// A temporary file representation which is deleted once disposed.
    /// </summary>
    public class TemporaryFile : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">The path to the temporary file</param>
        public TemporaryFile(string path)
        {
            this.Path = path;
        }
        
        /// <summary>
        /// Tries to delete the temporary file
        /// </summary>
        /// <returns>true if deletion is successful; false otherwise</returns>
        private bool Delete()
        {
            if (File.Exists(this.Path))
            {
                File.Delete(this.Path);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Temporary file path
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Releases the temporary file from resource management
        /// </summary>
        /// <returns>The path to the temporary file</returns>
        public string Release()
        {
            string path = this.Path;
            this.Path = null;
            return path;
        }

        #region IDisposable Support
        
        protected virtual void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty(this.Path))
            {
                if (disposing)
                {
                    Delete();
                }

                Release();
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
