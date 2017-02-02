// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;

using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Boost.Test;

using BoostTestAdapter.Utility.VisualStudio;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using BoostTestAdapter.Utility.ExecutionContext;

namespace BoostTestAdapter.Discoverers
{
    /// <summary>
    /// A Boost Test Discoverer that uses the output of the source executable called with --list_content=DOT parameter 
    /// to get the list of the tests.
    /// </summary>
    internal class ListContentDiscoverer : IBoostTestDiscoverer
    {
        #region Constructors

        /// <summary>
        /// Default constructor. A default implementation of IBoostTestRunnerFactory is provided.
        /// </summary>
        public ListContentDiscoverer()
            : this(new DefaultBoostTestRunnerFactory(), new DefaultVisualStudioInstanceProvider())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">A custom implementation of IBoostTestRunnerFactory.</param>
        /// <param name="vsProvider">Visual Studio Instance Provider</param>
        public ListContentDiscoverer(IBoostTestRunnerFactory factory, IVisualStudioInstanceProvider vsProvider)
        {
            _factory = factory;
            _vsProvider = vsProvider;
        }

        #endregion

        #region Members

        private readonly IBoostTestRunnerFactory _factory;
        private readonly IVisualStudioInstanceProvider _vsProvider;

        #endregion

        #region IBoostTestDiscoverer

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, ITestCaseDiscoverySink discoverySink)
        {
            Code.Require(sources, "sources");
            Code.Require(discoverySink, "discoverySink");

            // Populate loop-invariant attributes and settings

            BoostTestAdapterSettings settings = BoostTestAdapterSettingsProvider.GetSettings(discoveryContext);
            
            BoostTestRunnerSettings runnerSettings = new BoostTestRunnerSettings()
            {
                Timeout = settings.DiscoveryTimeoutMilliseconds
            };

            BoostTestRunnerCommandLineArgs args = new BoostTestRunnerCommandLineArgs()
            {
                ListContent = ListContentFormat.DOT
            };
            
            foreach (var source in sources)
            {
                try
                {
                    var vs = _vsProvider?.Instance;
                    if (vs != null)
                    {
                        Logger.Debug("Connected to Visual Studio {0} instance", vs.Version);
                    }

                    args.SetWorkingEnvironment(source, settings, vs);
                }
                catch (COMException ex)
                {
                    Logger.Exception(ex, "Could not retrieve WorkingDirectory from Visual Studio Configuration");
                }

                try
                {
                    IBoostTestRunner runner = _factory.GetRunner(source, settings.TestRunnerFactoryOptions);
                    using (TemporaryFile output = new TemporaryFile(TestPathGenerator.Generate(source, ".list.content.gv")))
                    {
                        // --list_content output is redirected to standard error
                        args.StandardErrorFile = output.Path;
                        Logger.Debug("list_content file: {0}", args.StandardErrorFile);

                        using (var context = new DefaultProcessExecutionContext())
                        { 
                            runner.Execute(args, runnerSettings, context);
                        }

                        // Skip sources for which the --list_content file is not available
                        if (!File.Exists(args.StandardErrorFile))
                        {
                            Logger.Error("--list_content=DOT output for {0} is not available. Skipping.", source);
                            continue;
                        }

                        // Parse --list_content=DOT output
                        using (FileStream stream = File.OpenRead(args.StandardErrorFile))
                        {
                            TestFrameworkDOTDeserialiser deserialiser = new TestFrameworkDOTDeserialiser(source);
                            TestFramework framework = deserialiser.Deserialise(stream);
                            if ((framework != null) && (framework.MasterTestSuite != null))
                            {
                                framework.MasterTestSuite.Apply(new VSDiscoveryVisitor(source, discoverySink));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Exception caught while discovering tests for {0} ({1} - {2})", source, ex.Message, ex.HResult);
                }
            }
        }

        #endregion IBoostTestDiscoverer
    }
}
