// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;

namespace BoostTestAdapter
{
    /// <summary>
    /// An interface to an object that provides support to the list-content feature.
    /// </summary>
    public interface IListContentHelper
    {
        /// <summary>
        /// Checks if the executable supports the --list_content parameter.
        /// </summary>
        /// <param name="exeName">The full path to the executable.</param>
        /// <returns></returns>
        bool IsListContentSupported(string exeName);

        /// <summary>
        /// Returns the output of the executable called with the --list_content parameter.
        /// </summary>
        /// <param name="exeName">The full path to the executable.</param>
        /// <returns></returns>
        string GetListContentOutput(string exeName);

        /// <summary>
        /// Returns an instance of a IDebugHelper.
        /// </summary>
        /// <param name="exeName">The full path to the executable.</param>
        /// <returns></returns>
        IDebugHelper CreateDebugHelper(string exeName);

        /// <summary>
        /// The timeout in milliseconds of all the processes started by the helper. Default: 5000.
        /// </summary>
        int Timeout { get; set; }
    }
}
