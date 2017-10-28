// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// An IBoostTestRunnerFactory wrapper which caches produced test runners
    /// </summary>
    public class CachingBoostTestRunnerFactory : IBoostTestRunnerFactory
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="factory">The base underlying factory which will produce the Boost.Test runners</param>
        public CachingBoostTestRunnerFactory(IBoostTestRunnerFactory factory)
        {
            BaseFactory = factory;

            _cache = new Dictionary<Tuple<string, BoostTestRunnerFactoryOptions>, IBoostTestRunner>();
        }

        #endregion

        #region Members

        /// <summary>
        /// Boost.Test runner cache
        /// </summary>
        private readonly Dictionary<Tuple<string, BoostTestRunnerFactoryOptions>, IBoostTestRunner> _cache;

        #endregion

        #region Properties

        /// <summary>
        /// Base (wrapped) factory
        /// </summary>
        public IBoostTestRunnerFactory BaseFactory { get; private set; }

        #endregion

        #region IBoostTestRunnerFactory
        
        public IBoostTestRunner GetRunner(string identifier, BoostTestRunnerFactoryOptions options)
        {
            var key = Tuple.Create(identifier, options);

            IBoostTestRunner runner = null;

            if (!_cache.TryGetValue(key, out runner))
            {
                runner = BaseFactory.GetRunner(identifier, options);
                _cache.Add(key, runner);
            }
            
            return runner;
        }

        #endregion
    }
}
