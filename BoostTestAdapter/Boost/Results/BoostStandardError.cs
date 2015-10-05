// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using BoostTestAdapter.Boost.Results.LogEntryTypes;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Standard Error as emitted by Boost Test executables
    /// </summary>
    public class BoostStandardError : BoostTestResultOutputBase
    {
        /// <summary>
        /// Constructor accepting a path to the external file
        /// </summary>
        /// <param name="path">The path to an external file. File will be opened on construction.</param>
        public BoostStandardError(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Constructor accepting a stream to the file contents
        /// </summary>
        /// <param name="stream">The file content stream.</param>
        public BoostStandardError(Stream stream)
            : base(stream)
        {
        }

        #region BoostTestResultOutputBase

        public override void Parse(TestResultCollection collection)
        {
            Utility.Code.Require(collection, "collection");

            string err = null;

            using (StreamReader reader = new StreamReader(this.InputStream))
            {
                err = reader.ReadToEnd();
            }

            if (!string.IsNullOrEmpty(err))
            {
                // Attach the stderr output to each TestCase result in the collection
                // since we cannot distinguish to which TestCase (in case multiple TestCases are registered)
                // the output is associated with.
                foreach (TestResult result in collection)
                {
                    // Consider the whole standard error contents as 1 entry.
                    result.LogEntries.Add(new LogEntryStandardErrorMessage(err));
                }
            }
        }

        #endregion BoostTestResultOutputBase
    }
}