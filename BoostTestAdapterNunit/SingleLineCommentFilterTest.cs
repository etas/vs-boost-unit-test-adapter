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
    public class SingleLineCommentFilterTest : SourceFilterTestBase
    {
        /// <summary>
        /// Tests the correct operation of the single line comments filter on C++ source files
        /// </summary>
        [Test]
        public void SingleLineCommentFilter()
        {
            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "SingleLineCommentFilterTest_UnFilteredSourceCode.cpp";
            const string filteredSourceCodeResourceName = "SingleLineCommentFilterTest_FilteredSourceCode.cpp";

            FilterAndCompareResources(
                new SingleLineCommentFilter(),
                null,
                nameSpace + unfilteredSourceCodeResourceName,
                nameSpace + filteredSourceCodeResourceName
            );
        }
    }
}
