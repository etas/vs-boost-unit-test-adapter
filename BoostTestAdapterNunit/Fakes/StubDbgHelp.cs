// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter.Utility;

namespace BoostTestAdapterNunit.Fakes
{
    /// <summary>
    /// A fake implementation of an IDebugHelper.
    /// It simulates the symbols of a typical Boost test executable.
    /// </summary>
    class StubDebugHelper : IDebugHelper
    {
        private readonly List<SymbolInfo> _symbolCache;

        public StubDebugHelper(string exeName)
        {
            _symbolCache = new List<SymbolInfo>();
            _symbolCache.AddRange(CreateFakeTestSymbols("", "Test1"));
            _symbolCache.AddRange(CreateFakeSuiteSymbols("UnitTest1", "UnitTest1"));
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest1", "Test11"));
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest1", "Test12"));
            _symbolCache.AddRange(CreateFakeSuiteSymbols("UnitTest2", "UnitTest2"));
            _symbolCache.AddRange(CreateFakeSuiteSymbols("UnitTest2", "UnitTest2::NestedUnitTest21"));
            _symbolCache.AddRange(CreateFakeSuiteSymbols("UnitTest2", "UnitTest2::NestedUnitTest21::NestedUnitTest211"));
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest2", "NestedUnitTest21::NestedUnitTest211::Test2111"));
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest2", "NestedUnitTest21::NestedUnitTest211::DisabledTest2112"));
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest2", "NestedUnitTest21::NestedUnitTest211::Test2113"));
            _symbolCache.AddRange(CreateFakeTestSymbols("", "Test2"));
            _symbolCache.AddRange(CreateFakeSuiteSymbols("Foo", "Foo"));
            _symbolCache.AddRange(CreateFakeTestSymbols("Foo", "Foo"));
        }
        
        public void Dispose()
        {
            _symbolCache.Clear();
        }

        #region IDebugHelper
        
        public SymbolInfo LookupSymbol(string name)
        {
            return _symbolCache.FirstOrDefault(sym => (sym.Name == name));
        }

        public bool ContainsSymbol(string name)
        {
            return LookupSymbol(name) != null;
        }

        #endregion IDebugHelper

        /// <summary>
        /// Creates a list of fake typical (found in a regular PDB) symbols for a test Suite.
        /// </summary>
        /// <param name="className">The suite name. It is also used as FileName for the symbols.</param>
        /// <param name="suiteName">The fully-qualyfied suite name.</param>
        /// <returns>A list of SymbolInfo.</returns>
        private IList<SymbolInfo> CreateFakeSuiteSymbols(string className, string suiteName)
        {
            var list = new List<SymbolInfo>();
            var rnd = new Random(12345);
            var address = rnd.Next(1, 9999999);
            var idx = rnd.Next(1, 999);
            var line = rnd.Next(1, 9999);

            list.Add(new SymbolInfo()
            {
                Address = (ulong)address,
                FileName = className + ".cpp",
                LineNumber = line + idx,
                Name = string.Format("{0}::`dynamic initializer for '{0}_registrar{1}''", suiteName, idx)
            });
            list.Add(new SymbolInfo()
            {
                Address = (ulong)address,
                FileName = className + ".cpp",
                LineNumber = line,
                Name = string.Format("{0}::`dynamic initializer for 'end_suite{1}_registrar{1}''", suiteName, line)
            });


            return list;

        }

        /// <summary>
        /// Creates a list of fake typical (found in a regular PDB) symbols for a test.
        /// </summary>
        /// <param name="className">The name of the first level suite that contains the test. It is also used as FileName for the symbols.</param>
        /// <param name="testName">The fully-qualyfied test name.</param>
        /// <returns>A list of SymbolInfo.</returns>
        private IList<SymbolInfo> CreateFakeTestSymbols(string className, string testName)
        {
            var list = new List<SymbolInfo>();
            var rnd = new Random(12345);
            var address = rnd.Next(1, 9999999);
            var idx = rnd.Next(1, 999);
            var line = rnd.Next(1, 9999);

            var prefix = string.IsNullOrEmpty(className) ? string.Empty : className + "::";

            list.Add(new SymbolInfo()
            {
                Address = (ulong)address,
                FileName = className + ".cpp",
                LineNumber = line,
                Name = string.Format("{0}`dynamic initializer for '{1}_registrar{2}''", prefix, testName, idx)
            });
            list.Add(new SymbolInfo()
            {
                Address = (ulong)address + 40000,
                FileName = className + ".cpp",
                LineNumber = line + 1,
                Name = string.Format("{0}{1}::test_method", prefix, testName)
            });
            list.Add(new SymbolInfo()
            {
                Address = (ulong)address + 20000,
                FileName = className + ".cpp",
                LineNumber = line,
                Name = string.Format("{0}{1}_invoker", prefix, testName)
            });

            return list;
        }

    }
}
