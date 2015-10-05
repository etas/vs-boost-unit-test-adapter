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
    class MultiLineCommentFilterTest : SourceFilterTestBase
    {
        /// <summary>
        /// Tests the correct operation (greediness wise) of the multiline comment filter
        /// </summary>
        [Test]
        public void MultiLineComment()
        {
            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "MultiLineCommentTest_UnFilteredSourceCode.cpp";
            const string filteredSourceCodeResourceName = "MultiLineCommentTest_FilteredSourceCode.cpp";

            FilterAndCompareResources(
                new MultilineCommentFilter(),
                null,
                nameSpace + unfilteredSourceCodeResourceName,
                nameSpace + filteredSourceCodeResourceName
            );
        }

    }
}
