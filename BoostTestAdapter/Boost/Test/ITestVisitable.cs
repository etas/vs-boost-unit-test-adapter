// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.Boost.Test
{
    /// <summary>
    /// Visitor design pattern interface intended for Boost.Test.TestUnit concrete implementations.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Visitable")]
    public interface ITestVisitable
    {
        /// <summary>
        /// Applies the test visitor over this instance.
        /// </summary>
        /// <param name="visitor">The visitor which will be visiting this instance.</param>
        void Apply(ITestVisitor visitor);
    }
}