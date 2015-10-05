// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.SourceFilter;
using NUnit.Framework;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// Base class which includes common functionality used throughout SourceFilerTests
    /// </summary>
    public class SourceFilterTestBase
    {
        /// <summary>
        /// Given 2 embedded resources locations, filters the left-hand resource and checks whether the filtered output matches that of the right-hand resource.
        /// </summary>
        /// <param name="filter">The source filter to apply</param>
        /// <param name="defines">The preprocessor definitions which are to be used by the source filter</param>
        /// <param name="lhsEmbeddedResource">The left-hand embedded resource fully qualified location whose content will be filtered</param>
        /// <param name="rhsEmbeddedResource">The right-hand embedded resource fully qualified location whose content is used to compare the filtered result</param>
        protected void FilterAndCompareResources(ISourceFilter filter, Defines defines, string lhsEmbeddedResource, string rhsEmbeddedResource)
        {
            FilterAndCompare(
                filter,
                defines,
                TestHelper.ReadEmbeddedResource(lhsEmbeddedResource),
                TestHelper.ReadEmbeddedResource(rhsEmbeddedResource)
            );
        }

        /// <summary>
        /// Given 2 strings, filters the left-hand string and checks whether the filtered output matches that of the right-hand string.
        /// </summary>
        /// <param name="filter">The source filter to apply</param>
        /// <param name="defines">The preprocessor definitions which are to be used by the source filter</param>
        /// <param name="lhs">The left-hand string whose value will be filtered</param>
        /// <param name="rhs">The right-hand string whose value is used to compare the filtered result</param>
        protected void FilterAndCompare(ISourceFilter filter, Defines defines, string lhs, string rhs)
        {
            var cppSourceFile = new CppSourceFile(){SourceCode = lhs};
            filter.Filter(cppSourceFile, defines);

            Assert.AreEqual(cppSourceFile.SourceCode, rhs);
        }
    }
}