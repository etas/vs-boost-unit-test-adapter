// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace BoostTestAdapter.Settings
{
    /// <summary>
    /// An include/exclude test source filter.
    /// </summary>
    [XmlRoot(XmlRootName)]
    public class TestSourceFilter
    {
        public const string XmlRootName = "Filters";

        /// <summary>
        /// Defines an empty filter collection
        /// </summary>
        public static readonly TestSourceFilter Empty = new TestSourceFilter();

        /// <summary>
        /// The test filter pattern white-list. Test source paths which match a filter are accepted.
        /// </summary>
        [XmlArray("Include")]
        [XmlArrayItem("Pattern")]
        public List<string> Include { get; set; }

        /// <summary>
        /// The test filter pattern black-list. Test source paths which match a filter are not accepted.
        /// </summary>
        [XmlArray("Exclude")]
        [XmlArrayItem("Pattern")]
        public List<string> Exclude { get; set; }

        /// <summary>
        /// States if this filter collection is empty i.e. contains no patterns
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return IsNullOrEmpty(this.Exclude) && IsNullOrEmpty(this.Include);
            }
        }

        /// <summary>
        /// Tests whether or not a given source file path is accepted based on the configured filters.
        /// </summary>
        /// <param name="source">The source file path</param>
        /// <returns>true if the test source is accepted; false otherwise</returns>
        public bool ShouldInclude(string source)
        {
            if (source == null)
            {
                return false;
            }

            // NOTE Due to ordering of statements, exclusions have precedence over inclusions
            //      which implies that if a test source matches both an include and an exclude
            //      pattern, the exclude pattern is preferred, thus the test would be rejected.

            // If a test source file-path matches one of the defined exclude patterns, reject test source
            if (!IsNullOrEmpty(this.Exclude) && IsMatch(this.Exclude, source))
            {
                return false;
            }

            // If a test source file-path matches one of the defined include patterns, accept test source
            if (!IsNullOrEmpty(this.Include) && IsMatch(this.Include, source))
            {
                return true;
            }

            // If no exclude/include patterns are defined, accept any and all tests
            // Else accept tests if-and-only-if an exclude list is present without an include list (i.e. 'accept all tests except...' semantics)
            return this.IsEmpty || (!IsNullOrEmpty(this.Exclude) && IsNullOrEmpty(this.Include));
        }

        /// <summary>
        /// Determines whether value matches any of the patterns specified in the 'patterns' collection
        /// </summary>
        /// <param name="patterns">Regulare expression pattern collection</param>
        /// <param name="value">The value to match</param>
        /// <returns>True if at least one pattern matches value</returns>
        private static bool IsMatch(IEnumerable<string> patterns, string value)
        {
            return patterns.Any(pattern => Regex.IsMatch(value, pattern));
        }

        /// <summary>
        /// List equivalent to System.String.IsNullOrEmpty
        /// </summary>
        /// <typeparam name="T">List element type</typeparam>
        /// <param name="list">The list to test</param>
        /// <returns>True if the provided list is null or empty; false otherwise</returns>
        private static bool IsNullOrEmpty<T>(List<T> list)
        {
            return ((list == null) || (list.Count == 0));
        }

        /// <summary>
        /// Determines whether the provided filter is a null instance or an empty instance
        /// </summary>
        /// <param name="filter">The filter to test</param>
        /// <returns>true if the provided instance is null or empty; false otherwise</returns>
        public static bool IsNullOrEmpty(TestSourceFilter filter)
        {
            return ((filter == null) || (filter.IsEmpty));
        }
    }
}