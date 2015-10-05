// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility.VisualStudio;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// A stub implementation for the IVisualStudioInstanceProvider interface.
    /// Provides an IVisualStudio instance which is defined at construction time.
    /// </summary>
    public class DummyVSProvider : IVisualStudioInstanceProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vs">The IVisualStudio instance which is to be provided on a 'Get()' call</param>
        public DummyVSProvider(IVisualStudio vs)
        {
            this.Instance = vs;
        }

        #region IVisualStudioInstanceProvider

        public IVisualStudio Instance { get; private set; }

        #endregion IVisualStudioInstanceProvider
    }

}
