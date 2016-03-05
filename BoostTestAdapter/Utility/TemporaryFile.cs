using System;
using System.IO;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// A temporary file representation which is deleted once disposed.
    /// </summary>
    class TemporaryFile : IDisposable
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
            if (!string.IsNullOrEmpty(this.Path) && File.Exists(this.Path))
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

        #region IDisposable Support

        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Delete();
                }

                _disposedValue = true;
            }
        }
        
        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
