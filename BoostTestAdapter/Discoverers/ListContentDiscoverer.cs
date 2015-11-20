// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace BoostTestAdapter.Discoverers
{
    /// <summary>
    /// A Boost Test Discoverer that uses the output of the source executable called with --list_content parameter 
    /// to get the list of the tests. 
    /// It also uses DbgHelperNative to complete the definition of each test with file name and line number.
    /// </summary>
    internal class ListContentDiscoverer : IBoostTestDiscoverer
    {
        #region Constructors

        /// <summary>
        /// Default constructor. A default implementation of IListContentHelper is provided.
        /// </summary>
        public ListContentDiscoverer()
            : this(new ListContentHelper())
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listContentHelper">A custom implementation of IListContentHelper.</param>
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
            Code.Require(sources, "sources");
            Code.Require(discoverySink, "discoverySink");

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(discoveryContext);
            _listContentHelper.Timeout = settings.DiscoveryTimeoutMilliseconds;

            foreach (var source in sources)
            {
                try
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
                            var unitName = l.Trim();
                            if (unitName.EndsWith("*", StringComparison.Ordinal))
                                unitName = unitName.Substring(0, unitName.Length - 1);

                            var currentLineIndentation = l.TrimEnd().LastIndexOf(' ') + 1;

                            // pop levels from the name builder to reach the current one
                            while (currentLineIndentation <= previousLineIndentation)
                            {
                                suiteNameBuilder.Pop();
                                previousLineIndentation -= 4;
                            }

                            // Retrieve all the symbols that contains <unitname> in their name.
                            // If no symbols can be retrieved, we skip the current unitName because 
                            // we cannot assume what kind of unit (test or suit) it is.
                            IEnumerable<SymbolInfo> syms;
                            if (!dbgHelp.LookupSymbol(unitName, out syms))
                                continue;

                            // Check if the unit is a Test or a Suite.
                            var testSymbol = GetTestSymbol(suiteNameBuilder, unitName, syms);
                            if (testSymbol == null)
                            {
                                // Suite
                                suiteNameBuilder.Push(unitName);

                                previousLineIndentation = currentLineIndentation;
                            }
                            else
                            {
                                // Test
                                var isEnabled = l.Contains("*");
                                var testCase = TestCaseUtils.CreateTestCase(
                                    source,
                                    new SourceFileInfo(testSymbol.FileName, testSymbol.LineNumber),
                                    suiteNameBuilder,
                                    unitName,
                                    isEnabled);
                                TestCaseUtils.AddTestCase(testCase, discoverySink);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception caught while discovering tests for {0} ({1} - {2})", source, ex.Message, ex.HResult);
                    Logger.Error(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Searches in a list of SymbolInfo the one that identifies a Test.
        /// </summary>
        /// <param name="suiteNameBuilder">Qualified name builder.</param>
        /// <param name="unitName">The name of the unit to be searched.</param>
        /// <param name="syms">The list of the symbols.</param>
        /// <returns>A SymbolInfo if <paramref name="unitName"/> is a test method.</returns>
        private static SymbolInfo GetTestSymbol(QualifiedNameBuilder suiteNameBuilder, string unitName, IEnumerable<SymbolInfo> syms)
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
                    "{0}::test_method",
                    fullyQualifiedName
                );

                return syms.FirstOrDefault(s => s.Name == symbolName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
