// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Collections.Generic;

using BoostTestAdapter.Settings;

using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class TestSourceFilterTest
    {
        #region Helper Methods

        private static TestSourceFilter CreateFilter(IEnumerable<string> inclusions, IEnumerable<string> exclusions)
        {
            return new TestSourceFilter
            {
                Include = ((inclusions == null) ? null : inclusions.ToList()),
                Exclude = ((exclusions == null) ? null : exclusions.ToList())
            };
        }

        #endregion Helper Methods

        #region Tests

        /// <summary>
        /// Test source filter with null include/exclude lists should accept everything.
        /// 
        /// Test aims:
        ///     - Ensure that a test source filter with null include/exclude lists accepts everything.
        /// </summary>
        [TestCase(@"D:\test.exe", Result = true)]
        [TestCase(@"test.exe", Result = true)]
        [TestCase("", Result = true)]
        [TestCase(null, Result = false)]
        public bool NullDefinitions(string source)
        {
            return CreateFilter(null, null).ShouldInclude(source);
        }

        /// <summary>
        /// Test source filter with empty include/exclude lists should accept everything.
        /// 
        /// Test aims:
        ///     - Ensure that a test source filter with empty include/exclude lists accepts everything.
        /// </summary>
        [TestCase(@"D:\test.exe", Result = true)]
        [TestCase(@"test.exe", Result = true)]
        [TestCase("", Result = true)]
        [TestCase(null, Result = false)]
        public bool EmptyDefinitions(string source)
        {
            return CreateFilter(Enumerable.Empty<string>(), Enumerable.Empty<string>()).ShouldInclude(source);
        }

        /// <summary>
        /// Test source filter with an exclude list should accept everything which is not match an exclude list pattern.
        /// 
        /// Test aims:
        ///     - Ensure that a test source filter with an exclude list should accept everything which is not match an exclude list pattern.
        /// </summary>
        [TestCase(new[] { @"test.exe$" }, @"D:\test.exe", Result = false)]
        [TestCase(new[] { @"test.exe$", @"my.test.exe$" }, @"D:\my.test.exe", Result = false)]
        [TestCase(new[] { @"no_match", @"test.exe$" }, @"D:\my.test.exe", Result = false, TestName = "ExcludeDefinitionOnly: Partial match exclusion - my.test.exe")]
        [TestCase(new[] { @"test.exe$", @"my.test.exe$" }, @"D:\main.exe", Result = true)]
        [TestCase(new[] { @"test.exe$" }, "", Result = true, TestName = "ExcludeDefinitionOnly: Include empty string")]
        [TestCase(new[] { "" }, "", Result = false, TestName = "ExcludeDefinitionOnly: Exclude empty string")]
        [TestCase(new[] { "test.exe$" }, null, Result = false)]
        public bool ExcludeDefinitionOnly(IEnumerable<string> exclusions, string source)
        {
            return CreateFilter(Enumerable.Empty<string>(), exclusions).ShouldInclude(source);
        }

        /// <summary>
        /// Test source filter with an include list should only accept tests which match an include list pattern.
        /// 
        /// Test aims:
        ///     - Ensure that a test source filter with an include list should only accept tests which match an include list pattern.
        /// </summary>
        [TestCase(new[] { @"test.exe$" }, @"D:\test.exe", Result = true)]
        [TestCase(new[] { @"test.exe$", @"my.test.exe$" }, @"D:\my.test.exe", Result = true)]
        [TestCase(new[] { @"no_match", @"test.exe$" }, @"D:\my.test.exe", Result = true, TestName = "IncludeDefinitionOnly: Partial match inclusion - my.test.exe")]
        [TestCase(new[] { @"test.exe$", @"my.test.exe$" }, @"D:\main.exe", Result = false)]
        [TestCase(new[] { @"test.exe$" }, "", Result = false, TestName = "IncludeDefinitionOnly: Exclude empty string")]
        [TestCase(new[] { "" }, "", Result = true, TestName = "IncludeDefinitionOnly: Include empty string")]
        [TestCase(new[] { "test.exe$" }, null, Result = false)]
        public bool IncludeDefinitionOnly(IEnumerable<string> inclusions, string source)
        {
            return CreateFilter(inclusions, Enumerable.Empty<string>()).ShouldInclude(source);
        }

        /// <summary>
        /// Test source filter with both an include and exclude list should only accept tests which match an include list pattern and do not match an exclude list pattern.
        /// 
        /// Test aims:
        ///     - Ensure that a test source filter with both an include and exclude list should only accept tests which match an include list pattern and do not match an exclude list pattern.
        /// </summary>
        [TestCase(new[] { @"test.exe$" }, new[] { @"test.exe$" }, @"D:\test.exe", Result = false)]
        [TestCase(new[] { @"test.exe$" }, new[] { @"mytest.exe$" }, @"D:\mytest.exe", Result = false)]
        [TestCase(new[] { @"test.exe$" }, new[] { @"mytest.exe$" }, @"D:\test.exe", Result = true, TestName = "ExcludeAndIncludeDefinitions: Exclude - test.exe")]
        [TestCase(new[] { @"" }, new[] { @"test.exe$" }, "", Result = true)]
        [TestCase(new[] { @"mytest.exe$" }, new[] { "" }, "", Result = false, TestName = "ExcludeAndIncludeDefinitions: Exclude empty string")]
        [TestCase(new[] { @"mytest.exe$" }, new[] { @"test.exe$" }, null, Result = false)]
        public bool ExcludeAndIncludeDefinitions(IEnumerable<string> inclusions, IEnumerable<string> exclusions, string source)
        {
            return CreateFilter(inclusions, exclusions).ShouldInclude(source);
        }

        #endregion

    }
}
