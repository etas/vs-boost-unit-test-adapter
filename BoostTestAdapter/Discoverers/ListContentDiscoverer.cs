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
        private const int _indentation = 4;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                            var isEnabled = false;

                            // Sanitize test unit name
                            var unitName = l.Trim();
                            if (unitName.EndsWith("*", StringComparison.Ordinal))
                            {
                                unitName = unitName.Substring(0, unitName.Length - 1).TrimEnd();
                                isEnabled = true;
                            }

                            // Identify indentation level
                            var currentLineIndentation = 0;
                            while (char.IsWhiteSpace(l[currentLineIndentation]))
                            {
                                ++currentLineIndentation;
                            }

                            // Pop levels from the name builder to reach the current one
                            while (currentLineIndentation <= previousLineIndentation)
                            {
                                suiteNameBuilder.Pop();
                                previousLineIndentation -= _indentation;
                            }
                            
                            // Try to locate a test case symbol for the test unit. If one is not found, assume it is a test suite.
                            var testSymbol = dbgHelp.LookupSymbol(BuildTestCaseSymbolName(suiteNameBuilder, unitName));
                            if (testSymbol == null)
                            {
                                // Suite
                                suiteNameBuilder.Push(unitName);

                                previousLineIndentation = currentLineIndentation;
                            }
                            else
                            {
                                // Test
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
                    Logger.Trace(ex.StackTrace);
                }
            }
        }
        
        /// <summary>
        /// Based on the parent test unit hierarchy and the provided test unit, generates a fully-qualified test case symbol name for the provided test unit.
        /// </summary>
        /// <param name="suiteNameBuilder">The parent test unit hierarchy.</param>
        /// <param name="unitName">The test unit.</param>
        /// <returns>The fully-qualified <b>test case</b> symbol name for the provided test unit.</returns>
        private static string BuildTestCaseSymbolName(QualifiedNameBuilder suiteNameBuilder, string unitName)
        {
            var fullyQualifiedName = unitName;

            string testUnitLocator = suiteNameBuilder.ToString();

            // If the test is located within a test suite, qualify accordingly
            if (!string.IsNullOrEmpty(testUnitLocator))
            { 
                fullyQualifiedName = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}::{1}",
                    testUnitLocator.Replace("/", "::"),
                    unitName
                );
            }

            var symbolName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}::test_method",
                fullyQualifiedName
            );
            
            return symbolName;
        }

        #endregion
    }
}
