// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Globalization;
using System.Linq;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;
using BoostTestAdapter.Utility.ExecutionContext;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// Configurable External Boost Test Runner. Launches a process as specified
    /// in the provided configuration to execute tests.
    /// </summary>
    public class ExternalBoostTestRunner : BoostTestRunnerBase
    {
        #region Constants

        private const string SourcePlaceholder = "source";
        private const string TimeoutPlaceholder = "timeout";
        private const string BoostArgsPlaceholder = "boost-args";

        #endregion Constants

        #region Members

        private string _source = null;

        #endregion Members

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The test source (dll/exe) for which this external test runner will execute</param>
        /// <param name="settings">External test runner configuration</param>
        public ExternalBoostTestRunner(string source, ExternalBoostTestRunnerSettings settings) :
            base(GetTestExecutable(settings, source))
        {
            this._source = source;
            this.Settings = settings;
        }

        public ExternalBoostTestRunnerSettings Settings { get; private set; }

        #region IBoostTestRunner

        public override string Source
        {
            get
            {
                return this._source;
            }
        }

        public override bool ListContentSupported
        {
            get
            {
                return (Settings.DiscoveryMethodType == DiscoveryMethodType.DiscoveryListContent);
            }
        }

        public override bool VersionSupported { get; } = false;

        #endregion IBoostTestRunner

        #region BoostTestRunnerBase

        /// <summary>
        /// Provides a ProcessExecutionContextArgs structure containing the necessary information to launch the test process.
        /// Aggregates the BoostTestRunnerCommandLineArgs structure with the command-line arguments specified at configuration stage.
        /// </summary>
        /// <param name="args">The Boost Test Framework command line arguments</param>
        /// <param name="settings">The Boost Test Runner settings</param>
        /// <returns>A valid ProcessExecutionContextArgs structure to launch the test executable</returns>
        protected override ProcessExecutionContextArgs GetExecutionContextArgs(BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
        {
            Code.Require(args, "args");

            ProcessExecutionContextArgs info = base.GetExecutionContextArgs(args, settings);

            BoostTestRunnerCommandLineArgs tmpArgs = args.Clone();
            tmpArgs.StandardErrorFile = null;
            tmpArgs.StandardOutFile = null;

            CommandEvaluator evaluator = BuildEvaluator(this.Source, tmpArgs, settings);
            CommandEvaluationResult result = evaluator.Evaluate(this.Settings.ExecutionCommandLine.Arguments);
            
            string cmdLineArgs = result.Result;
            if (!result.MappedVariables.Contains(BoostArgsPlaceholder))
            {
                cmdLineArgs = result.Result + (result.Result.EndsWith(" ", StringComparison.Ordinal) ? string.Empty : " ") + args.ToString();
            }

            BoostTestRunnerCommandLineArgs redirection = new BoostTestRunnerCommandLineArgs
            {
                StandardOutFile = args.StandardOutFile,
                StandardErrorFile = args.StandardErrorFile
            };
            cmdLineArgs += redirection.ToString();
           
            info.FilePath = evaluator.Evaluate(this.Settings.ExecutionCommandLine.FileName).Result;
            info.Arguments = cmdLineArgs;

            return info;
        }

        #endregion BoostTestRunnerBase

        /// <summary>
        /// Extracts and evaluates the test executable from the external Boost Test runner configuration.
        /// </summary>
        /// <param name="settings">The external Boost Test runner configuration</param>
        /// <param name="source">The test source module containing the tests to execute</param>
        /// <returns>The evaluated, test executable program string</returns>
        private static string GetTestExecutable(ExternalBoostTestRunnerSettings settings, string source)
        {
            Utility.Code.Require(settings, "settings");
            Utility.Code.Require(settings.ExecutionCommandLine, "settings.ExecutionCommandLine");

            return BuildEvaluator(source).Evaluate(settings.ExecutionCommandLine.FileName).Result;
        }
        
        /// <summary>
        /// Provides a preset CommandEvaluator instance for evaluating strings containing the source placeholder.
        /// </summary>
        /// <param name="source">The source placeholder value</param>
        /// <returns>A CommandEvaluator instance for evaluating strings containing the source placeholder.</returns>
        private static CommandEvaluator BuildEvaluator(string source)
        {
            CommandEvaluator evaluator = new CommandEvaluator();

            evaluator.SetVariable(SourcePlaceholder, "\"" + source + "\"");

            return evaluator;
        }

        /// <summary>
        /// Provides a preset CommandEvaluator instance for evaluating strings containing the source, timeout and boost-args placeholders.
        /// </summary>
        /// <param name="source">The source placeholder value</param>
        /// <param name="args">The boost arguments placeholder value</param>
        /// <param name="settings">The test runner settings which contains the timeout placeholder value</param>
        /// <returns>A CommandEvaluator instance for evaluating strings containing the source, timeout and boost-args placeholders.</returns>
        private static CommandEvaluator BuildEvaluator(string source, BoostTestRunnerCommandLineArgs args, BoostTestRunnerSettings settings)
        {
            CommandEvaluator evaluator = BuildEvaluator(source);

            evaluator.SetVariable(TimeoutPlaceholder, Math.Max(0, settings.Timeout).ToString(CultureInfo.InvariantCulture));
            evaluator.SetVariable(BoostArgsPlaceholder, args.ToString());

            return evaluator;
        }
    }
}
