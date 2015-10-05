// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Base class for IBoostTestResultOutput implementations
    /// providing common functionality.
    /// </summary>
    public abstract class BoostTestResultOutputBase : IBoostTestResultOutput
    {
        #region Constructors

        /// <summary>
        /// Constructor for external files.
        /// </summary>
        /// <param name="path">The path to an external file. File will be opened on construction.</param>
        protected BoostTestResultOutputBase(string path)
        {
            this.CloseStreamOnDispose = true;
            this.InputStream = File.OpenRead(path);
            this.IsDisposed = false;
        }

        /// <summary>
        /// Constructor for streams. Ideal for test purposes.
        /// </summary>
        /// <param name="stream">The stream containing the output.</param>
        protected BoostTestResultOutputBase(Stream stream)
        {
            this.CloseStreamOnDispose = false;
            this.InputStream = stream;

            this.IsDisposed = false;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Flag stating whether the stream should be closed on dispose.
        /// </summary>
        protected bool CloseStreamOnDispose { get; set; }

        /// <summary>
        /// The input stream representing the content.
        /// </summary>
        protected Stream InputStream { get; set; }

        #endregion Properties

        #region IBoostOutputParser

        public abstract void Parse(TestResultCollection collection);

        #endregion IBoostOutputParser

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (this.CloseStreamOnDispose)
                    {
                        this.InputStream.Dispose();
                    }
                }
            }

            IsDisposed = true;
        }

        private bool IsDisposed { get; set; }

        #endregion IDisposable
    }
}