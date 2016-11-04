// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Base class for IBoostTestResultOutput implementations
    /// providing common functionality.
    /// </summary>
    public abstract class BoostTestResultOutputBase : IBoostTestResultParser
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The destination result collection. Possibly used for result aggregation.</param>
        protected BoostTestResultOutputBase(IDictionary<string, TestResult> target)
        {
            this.Target = target ?? new Dictionary<string, TestResult>();
        }
        
        #endregion Constructors

        #region Properties
        
        /// <summary>
        /// The input stream representing the content.
        /// </summary>
        protected IDictionary<string, TestResult> Target { get; private set; }

        #endregion Properties

        #region IBoostOutputParser

        public virtual IDictionary<string, TestResult> Parse(string content)
        {
            return Target;
        }

        #endregion IBoostOutputParser
    }
}