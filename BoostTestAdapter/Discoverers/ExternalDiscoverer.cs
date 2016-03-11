// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using System.Xml.Serialization;
using BoostTestAdapter.Boost.Test;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using BoostTestAdapter.Boost.Runner;
using BoostTestAdapter.Utility.VisualStudio;

namespace BoostTestAdapter.Discoverers
{
    /// <summary>
    /// A Boost Test Discoverer which discovers tests based on configuration.
    /// </summary>
    internal class ExternalDiscoverer : IBoostTestDiscoverer
    {
        #region Constants

        private const string ListFileSuffix = ".test.list.xml";

        #endregion Constants

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Settings for this instance of the discoverer.</param>
        public ExternalDiscoverer(ExternalBoostTestRunnerSettings settings)
            : this(settings, new DefaultVisualStudioInstanceProvider())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Settings for this instance of the discoverer.</param>
        /// <param name="provider">Visual Studio Instance provider</param>
        public ExternalDiscoverer(ExternalBoostTestRunnerSettings settings, IVisualStudioInstanceProvider provider)
        {
            Settings = settings;
            VSProvider = provider;
        }

        /// <summary>
        /// Settings for this instance of the discoverer.
        /// </summary>
        public ExternalBoostTestRunnerSettings Settings { get; private set; }

        /// <summary>
        /// Visual Studio Instance Provider
        /// </summary>
        public IVisualStudioInstanceProvider VSProvider { get; private set; }

        #region IBoostTestDiscoverer

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, ITestCaseDiscoverySink discoverySink)
        {
            Code.Require(sources, "sources");
            Code.Require(discoverySink, "discoverySink");

            if (this.Settings.DiscoveryMethodType == DiscoveryMethodType.DiscoveryListContent)
            {
                // Delegate to ListContentDiscoverer
                ListContentDiscoverer discoverer = new ListContentDiscoverer(new ExternalBoostTestRunnerFactory(), VSProvider);
                discoverer.DiscoverTests(sources, discoveryContext, discoverySink);
            }            
            else
            {
                foreach (string source in sources)
                {
                    TestFramework framework = DiscoverTestFramework(source);

                    if ((framework != null) && (framework.MasterTestSuite != null))
                    {
                        VSDiscoveryVisitor visitor = new VSDiscoveryVisitor(source, discoverySink);
                        framework.MasterTestSuite.Apply(visitor);
                    }
                }
            }
        }

        #endregion IBoostTestDiscoverer

        /// <summary>
        /// Based on the established configuration, discovers the tests within the provided test source module.
        /// </summary>
        /// <param name="source">The test source module</param>
        /// <returns>The test framework describing all tests contained within the test source or null if one cannot be provided.</returns>
        private TestFramework DiscoverTestFramework(string source)
        {
            if (Settings.DiscoveryMethodType == DiscoveryMethodType.DiscoveryFileMap)
            {
                return ParseStaticTestList(source);
            }
            else if (Settings.DiscoveryMethodType == DiscoveryMethodType.DiscoveryCommandLine)
            {
                return ExecuteExternalDiscoveryCommand(source);
            }

            return null;
        }
        
        /// <summary>
        /// Executes the discovery command as specified in the configuration for the requested test source.
        /// </summary>
        /// <param name="source">The test source module</param>
        /// <returns>The test framework describing all tests contained within the test source or null if one cannot be provided.</returns>
        private TestFramework ExecuteExternalDiscoveryCommand(string source)
        {
            // Use a temporary file to host the result of the external discovery process
            string path = Path.Combine(Path.GetDirectoryName(source), Path.GetFileName(source) + ListFileSuffix);

            // Perform cleanup to avoid inconsistent listing
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            CommandEvaluator evaluator = new CommandEvaluator();

            evaluator.SetVariable("source", "\"" + source + "\"");
            evaluator.SetVariable("out", "\"" + path + "\"");

            // Evaluate the discovery command
            CommandLine commandLine = new CommandLine
            {
                FileName = evaluator.Evaluate(Settings.DiscoveryCommandLine.FileName).Result,
                Arguments = evaluator.Evaluate(Settings.DiscoveryCommandLine.Arguments).Result
            };

            // Execute the discovery command via an external process
            if (ExecuteCommand(commandLine))
            {
                // Parse the generate TestFramework from the temporary file
                return ParseTestFramework(path);
            }

            return null;
        }

        /// <summary>
        /// Executes the provided command line as an external process.
        /// </summary>
        /// <param name="commandLine">The process command line</param>
        /// <returns>true if the process terminated successfully (exit code = 0); false otherwise.</returns>
        private static bool ExecuteCommand(CommandLine commandLine)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetTempPath(),
                FileName = commandLine.FileName,
                Arguments = commandLine.Arguments,
                RedirectStandardError = false,
                RedirectStandardInput = false
            };
            
            Process process = Process.Start(ProcessStartInfoEx.StartThroughCmdShell(info));
            if (process != null)
            {
                process.WaitForExit(60000);

                return (process.ExitCode == 0);
            }

            return false;
        }

        /// <summary>
        /// Parses a static file containing the test listing for the requested test source as specified in the configuration.
        /// </summary>
        /// <param name="source">The test source module</param>
        /// <returns>The test framework describing all tests contained within the test source or null if one cannot be provided.</returns>
        private TestFramework ParseStaticTestList(string source)
        {
            string path;
            if (Settings.DiscoveryFileMap.TryGetValue(Path.GetFileName(source), out path))
            {
                return ParseTestFramework(path);   
            }

            return null;
        }

        /// <summary>
        /// Deserializes a TestFramework from the Xml file path provided.
        /// </summary>
        /// <param name="path">A valid path to a TestFramework Xml file.</param>
        /// <returns>The deserialized TestFramework</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static TestFramework ParseTestFramework(string path)
        {
            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(TestFramework));
                    return deserializer.Deserialize(stream) as TestFramework;
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "Exception caught while reading xml file {0} ({1} -  {2})", path, ex.Message, ex.HResult);
            }
            return null;
        }

        /// <summary>
        /// Internal IBoostTestRunnerFactory implementation which
        /// exclusively produces ExternalBoostTestRunner instances.
        /// </summary>
        private class ExternalBoostTestRunnerFactory : IBoostTestRunnerFactory
        {
            #region IBoostTestRunnerFactory

            public IBoostTestRunner GetRunner(string source, BoostTestRunnerFactoryOptions options)
            {
                Code.Require(source, "source");
                Code.Require(options, "options");
                Code.Require(options.ExternalTestRunnerSettings, "options.ExternalTestRunnerSettings");
                
                return new ExternalBoostTestRunner(source, options.ExternalTestRunnerSettings);
            }

            #endregion IBoostTestRunnerFactory
        }
    }
}
