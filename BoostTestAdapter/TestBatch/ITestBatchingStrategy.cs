// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using BoostTestAdapter.Utility;

using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace BoostTestAdapter.TestBatch
{
    /// <summary>
    /// Defines a test batching strategy. Groups a collection of tests into a 
    /// series of test runs for eventual execution.
    /// </summary>
    public interface ITestBatchingStrategy
    {
        /// <summary>
        /// Groups the provided test cases into a series of test run executions.
        /// </summary>
        /// <param name="tests">The tests which are to be executed</param>
        /// <returns>An enumeration of test runs, possibly aggregating multiple test cases into a single test run</returns>
        IEnumerable<TestRun> BatchTests(IEnumerable<VSTestCase> tests);
    }
}