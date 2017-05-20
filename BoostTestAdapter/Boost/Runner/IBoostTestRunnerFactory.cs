// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Abstract Factory which provides IBoostTestRunner instances.
    /// </summary>
    public interface IBoostTestRunnerFactory
    {
        /// <summary>
        /// Returns an IBoostTestRunner based on the provided identifier.
        /// </summary>
        /// <param name="identifier">A unique identifier able to distinguish different BoostTestRunner types.</param>
        /// <param name="options">A structure which states particular features of interest in the manufactured product.</param>
        /// <returns>An IBoostTestRunner instance or null if one cannot be provided.</returns>
        IBoostTestRunner GetRunner(string identifier, BoostTestRunnerFactoryOptions options);
    }
}