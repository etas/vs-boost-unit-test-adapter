// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Default implementation of IBoostTestRunnerCapabilities
    /// </summary>
    public class BoostTestRunnerCapabilities : IBoostTestRunnerCapabilities
    {
        #region IBoostTestRunnerCapabilities

        public bool ListContent { get; set; } = false;

        public bool Version { get; set; } = false;

        #endregion
    }
}
