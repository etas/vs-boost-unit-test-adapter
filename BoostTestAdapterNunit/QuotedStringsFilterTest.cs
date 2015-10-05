// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.SourceFilter;
using BoostTestAdapterNunit.Utility;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class QuotedStringsFilterTest : SourceFilterTestBase
    {
        /// <summary>
        /// Tests the correct operation of the quoted strings filter on C++ source files
        /// </summary>
        [Test]
        public void QuotedStringsFilter()
        {
            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "QuotedStringsFilterTest_UnFilteredSourceCode.cpp";
            const string filteredSourceCodeResourceName = "QuotedStringsFilterTest_FilteredSourceCode.cpp";

            FilterAndCompareResources(
                new QuotedStringsFilter(),
                null,
                nameSpace + unfilteredSourceCodeResourceName,
                nameSpace + filteredSourceCodeResourceName
            );
        }
    }
}
