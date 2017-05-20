// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Utility class to reduce boilerplate code
    /// and to comply with code analysis.
    /// </summary>
    internal static class Code
    {
        /// <summary>
        /// Asserts that an object is not null.
        /// </summary>
        /// <param name="arg">The argument to test</param>
        /// <param name="argName">The name of arg</param>
        /// <exception cref="ArgumentNullException">Thrown if arg is null</exception>
        public static void Require(object arg, string argName)
        {
            if (arg == null) throw new ArgumentNullException(argName);
        }
    }
}
