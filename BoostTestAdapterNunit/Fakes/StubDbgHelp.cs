using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoostTestAdapter.Utility;

namespace BoostTestAdapterNunit.Fakes
{
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
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest2", "UnitTest2::NestedUnitTest21::NestedUnitTest211::Test2111"));
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest2", "UnitTest2::NestedUnitTest21::NestedUnitTest211::DisabledTest2112"));
            _symbolCache.AddRange(CreateFakeTestSymbols("UnitTest2", "UnitTest2::NestedUnitTest21::NestedUnitTest211::Test2113"));
            _symbolCache.AddRange(CreateFakeTestSymbols("", "Test2"));
        }

        public void Dispose()
        {
            _symbolCache.Clear();
        }

        public bool LookupSymbol(string name, out IEnumerable<SymbolInfo> symbols)
        {
            symbols = _symbolCache.Where(s => s.Name.Contains(name));
            if (symbols.Any())
                return true;

            return false;
        }

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

        private IList<SymbolInfo> CreateFakeTestSymbols(string className, string testName)
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
                LineNumber = line,
                Name = string.Format("{0}::`dynamic initializer for '{1}_registrar{2}''", className, testName, idx)
            });
            list.Add(new SymbolInfo()
            {
                Address = (ulong)address + 40000,
                FileName = className + ".cpp",
                LineNumber = line + 1,
                Name = string.Format("{0}::{1}::test_method", className, testName)
            });
            list.Add(new SymbolInfo()
            {
                Address = (ulong)address + 20000,
                FileName = className + ".cpp",
                LineNumber = line,
                Name = string.Format("{0}::{1}_invoker", className, testName)
            });

            return list;
        }

    }
}
