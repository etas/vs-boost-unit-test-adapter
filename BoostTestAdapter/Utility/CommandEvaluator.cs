// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Result structure for command evaluation.
    /// </summary>
    public class CommandEvaluationResult
    {
        public CommandEvaluationResult(string result, IEnumerable<string> variables)
        {
            this.Result = result;
            this.MappedVariables = variables;
        }

        /// <summary>
        /// The resulting string after command evaluation.
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// A listing of all variables which were successfully mapped during command evaluation.
        /// </summary>
        public IEnumerable<string> MappedVariables { get; private set; }
    }

    /// <summary>
    /// Allows for string substution following a format similar to: A {variable} to be substituted.
    /// </summary>
    public class CommandEvaluator
    {
        #region Members

        private IDictionary<string, string> _variables = null;

        #endregion Members

        #region Constructors

        public CommandEvaluator()
        {
            this._variables = new Dictionary<string, string>();
        }

        #endregion Constructors

        /// <summary>
        /// Specifies a value for the given variable placeholder.
        /// </summary>
        /// <param name="variable">The variable placeholder label.</param>
        /// <param name="value">The respective variable value.</param>
        public void SetVariable(string variable, string value)
        {
            this[variable] = value;
        }

        /// <summary>
        /// Removes the mapped variable.
        /// </summary>
        /// <param name="variable">The variable placeholder label to remove.</param>
        public void ResetVariable(string variable)
        {
            _variables.Remove(variable);
        }

        public string this[string variable]
        {
            get
            {
                string value = null;
                return (_variables.TryGetValue(variable, out value)) ? value : null;
            }

            set
            {
                _variables[variable] = value;
            }
        }

        /// <summary>
        /// Evaluates the given string for any and all variable placeholders.
        /// </summary>
        /// <param name="input">The string to evaluate containing variable placeholders</param>
        /// <returns>A CommandEvaluationResult detailing the result of the evaluation process</returns>
        public CommandEvaluationResult Evaluate(string input)
        {
            string evaluation = input;

            ISet<string> mappedVariables = new HashSet<string>();

            foreach (KeyValuePair<string, string> entry in this._variables)
            {
                KeyValuePair<string, string> entryRef = entry;

                Regex variable = GetVariablePlaceholderRegex(entry.Key);
                evaluation = variable.Replace(evaluation, (match) =>
                {
                    mappedVariables.Add(entryRef.Key);
                    return (entryRef.Value ?? "null");
                });
            }

            return new CommandEvaluationResult(evaluation, mappedVariables);
        }

        /// <summary>
        /// Provides a Regex which can identify the provided variable placeholder
        /// </summary>
        /// <param name="variable">The variable placeholder to capture</param>
        /// <returns>A Regex which can identify the provided variable placeholder</returns>
        private static Regex GetVariablePlaceholderRegex(string variable)
        {
            return new Regex('{' + Regex.Escape(variable) + '}');
        }
    }
}
