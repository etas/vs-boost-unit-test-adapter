using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using BoostTestAdapter.Utility;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace BoostTestAdapter.Discoverers
{
    internal class ListContentDiscoverer : IBoostTestDiscoverer
    {
        #region Constructors

        public ListContentDiscoverer()
            : this(new ListContentHelper())
        {

        }

        public ListContentDiscoverer(IListContentHelper listContentHelper)
        {
            _listContentHelper = listContentHelper;
        }

        #endregion


        #region Members

        private readonly IListContentHelper _listContentHelper;

        #endregion


        #region IBoostTestDiscoverer

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            if (sources == null || !sources.Any())
                return;

            foreach (var source in sources)
            {
                var output = _listContentHelper.GetListContentOutput(source);

                using (var dbgHelp = _listContentHelper.CreateDebugHelper(source))
                {
                    QualifiedNameBuilder suiteNameBuilder = new QualifiedNameBuilder();
                    suiteNameBuilder.Push(QualifiedNameBuilder.DefaultMasterTestSuiteName);

                    var previousLineIndentation = -1;
                    var lines = output.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var l in lines)
                    {
                        var unitName = l.Trim().Replace("*", string.Empty);

                        var currentLineIndentation = l.TrimEnd().LastIndexOf(' ') + 1;
                        while (currentLineIndentation <= previousLineIndentation)
                        {
                            suiteNameBuilder.Pop();
                            previousLineIndentation -= 4;
                        }

                        IEnumerable<SymbolInfo> syms;
                        if (!dbgHelp.LookupSymbol(unitName, out syms))
                            continue;

                        var isSuite = IsSuite(suiteNameBuilder, unitName, syms);
                        if (isSuite)
                        {
                            suiteNameBuilder.Push(unitName);

                            previousLineIndentation = currentLineIndentation;
                        }
                        else
                        {
                            var sym = syms.FirstOrDefault(s => s.Name.EndsWith("test_method", StringComparison.Ordinal));
                            if (sym != null)
                            {
                                var isEnabled = l.Contains("*");
                                var testCase = Helper.CreateTestCase(
                                    source,
                                    new SourceFileInfo(sym.FileName, sym.LineNumber),
                                    suiteNameBuilder,
                                    unitName,
                                    isEnabled);
                                Helper.AddTestCase(testCase, discoverySink);
                            }
                        }
                    }
                }
            }
        }

        private static bool IsSuite(QualifiedNameBuilder suiteNameBuilder, string unitName, IEnumerable<SymbolInfo> syms)
        {
            try
            {
                var fullyQualifiedName = unitName;
                if (!string.IsNullOrEmpty(suiteNameBuilder.ToString()))
                    fullyQualifiedName = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}::{1}",
                        suiteNameBuilder.ToString().Replace("/", "::"),
                        unitName
                    );

                var symbolName = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}::`dynamic initializer for 'end_suite",
                    fullyQualifiedName
                );

                var suiteMatch = syms.FirstOrDefault(s => s.Name.Contains(symbolName));

                symbolName = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}::test_method",
                    fullyQualifiedName
                );

                var testMatch = syms.FirstOrDefault(s => s.Name.Contains(symbolName));

                if (suiteMatch != null && testMatch == null)
                    return true;

                if (suiteMatch == null && testMatch != null)
                    return false;

                var idxEnd = suiteMatch.Name.LastIndexOf("::", StringComparison.Ordinal);
                var idxStart = suiteMatch.Name.LastIndexOf("::", idxEnd - 1, StringComparison.Ordinal);

                if (idxStart == -1)
                    idxStart = -2;

                var suiteIdentifier = suiteMatch.Name.Substring(idxStart + 2, idxEnd - idxStart - 2);

                idxEnd = testMatch.Name.LastIndexOf("::", StringComparison.Ordinal);
                idxStart = testMatch.Name.LastIndexOf("::", idxEnd - 1, StringComparison.Ordinal);

                if (idxStart == -1)
                    idxStart = -2;

                var testIdentifier = testMatch.Name.Substring(idxStart + 2, idxEnd - idxStart - 2);

                if (testIdentifier.Length - unitName.Length > suiteIdentifier.Length - unitName.Length)
                    return true;

                return false;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        #endregion
    }
}
