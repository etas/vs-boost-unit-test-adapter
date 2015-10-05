// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using VisualStudioAdapter;

namespace BoostTestAdapter.Utility.VisualStudio
{
    /// <summary>
    /// Abstract factory which provides IVisualStudio instances.
    /// </summary>
    public interface IVisualStudioInstanceProvider
    {
        /// <summary>
        /// Provides an IVisualStudio instance.
        /// </summary>
        /// <returns>An IVisualStudio instance or null if provisioning is not possible.</returns>
        IVisualStudio Instance { get; }
    }
}