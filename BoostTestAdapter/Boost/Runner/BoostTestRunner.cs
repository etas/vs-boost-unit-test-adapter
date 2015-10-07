// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// IBoostTestRunner implementation. Executes stand-alone
    /// (i.e. test runner included within '.exe') Boost Tests.
    /// </summary>
    public class BoostTestRunner : BoostTestRunnerBase
    {
        public BoostTestRunner(string exe) :
            base(exe)
        {
        }
    }
}