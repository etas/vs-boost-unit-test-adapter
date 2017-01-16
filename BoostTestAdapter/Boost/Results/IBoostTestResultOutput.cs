// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Interface for Boost Test result output.
    /// </summary>
    public interface IBoostTestResultParser
    {
        /// <summary>
        /// Parses the referenced output and provides a test result collection containing the parsed result.
        /// </summary>
        /// <param name="content">The report content as a string.</param>
        /// <returns>A test result collection containing the parsed output</returns>
        IDictionary<string, TestResult> Parse(string content);
    }
}