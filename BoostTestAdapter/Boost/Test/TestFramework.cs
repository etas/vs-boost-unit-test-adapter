// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.Boost.Test
{
    public class TestFramework
    {
        #region Constructors

        public TestFramework() :
            this(null, null)
        {
        }

        public TestFramework(string source, TestSuite master)
        {
            this.Source = source;
            this.MasterTestSuite = master;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Fully qualified path detailing the source Dll/EXE which contains these tests
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Boost Test Master Test Suite
        /// </summary>
        public TestSuite MasterTestSuite { get; private set; }

        #endregion Properties
    }
}