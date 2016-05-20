// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.SourceFilter;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class SourceFiltersTest
    {
        /// <summary>
        /// Given an source line, the QuotedStringFilter filters out any quoted text.
        /// 
        /// Test aims:
        ///     - Ensure that the QuotedStringsFilter filters out quoted text as expected.
        /// </summary>
        [TestCase("std::cout << \"hello world\" << std::endl;", Result = "std::cout <<  << std::endl;")]
        [TestCase("BOOST_MESSAGE(\"This is a \\\"test\");", Result = "BOOST_MESSAGE();")]
        [TestCase("BOOST_MESSAGE(\"This is a \\\"test\",\"\");", Result = "BOOST_MESSAGE(,);")]
        [TestCase("BOOST_MESSAGE(\"This is a \\\"test\",\"This is a test\");", Result = "BOOST_MESSAGE(,);")]
        [TestCase("BOOST_MESSAGE(\"This is a test\\\"\");", Result = "BOOST_MESSAGE();")]
        [TestCase("BOOST_MESSAGE(\"This is a just a\\\" test\");", Result = "BOOST_MESSAGE();")]
        [TestCase("#include \"stdafx.h\"", Result = "#include ")]
        [TestCase("const char* const cikku = \"Hello /\r\nWorld\"", Result = "const char* const cikku = \r\n")]
        public string FilterQuotedString(string input)
        {
            ISourceFilter filter = new QuotedStringsFilter();
            CppSourceFile cppSourceFile = new CppSourceFile(){SourceCode = input};
            filter.Filter(cppSourceFile, null);
            return cppSourceFile.SourceCode;
        }

    }
}
