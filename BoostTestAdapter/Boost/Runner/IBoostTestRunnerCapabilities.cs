// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Aggregation of a Boost.Test runner's capabilities
    /// </summary>
    public interface IBoostTestRunnerCapabilities
    {
        /// <summary>
        /// Determines if the Boost.Test runner supports the '--list_content' command-line argument
        /// </summary>
        bool ListContent { get; }
        
        /// <summary>
        /// Determines if the Boost.Test runner supports the '--version' command-line argument
        /// </summary>
        bool Version { get; }
    }
}
