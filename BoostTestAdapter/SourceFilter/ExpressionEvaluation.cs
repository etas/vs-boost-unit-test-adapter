// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Globalization;
using NCalc;
using NCalc.Domain;
using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Evaluates an expression using NCalc
    /// </summary>
    /// <see cref="https://ncalc.codeplex.com/"></see>
    public class ExpressionEvaluation : IEvaluation
    {
        #region Members

        private Defines _definesHandler;

        #endregion Members

        #region IEvaluation

        /// <summary>
        /// Evaluates an expression
        /// </summary>
        /// <param name="expression">expression to be evaluated</param>
        /// <param name="definesHandler">reference to the defines handler </param>
        /// <returns></returns>
        public EvaluationResult EvaluateExpression(string expression, Defines definesHandler)
        {
            this._definesHandler = definesHandler;

            Expression e = new Expression(expression, EvaluateOptions.NoCache);
            e.EvaluateParameter += EvaluateParam;
            e.EvaluateFunction += EvaluateFunction;

            EvaluationResult evaluationResult = EvaluationResult.UnDetermined;

            try
            {
                object result = e.Evaluate();

                evaluationResult = Convert.ToBoolean(result, CultureInfo.InvariantCulture) ? EvaluationResult.IsTrue : EvaluationResult.IsFalse;
            }
            catch
            {
                evaluationResult = EvaluationResult.UnDetermined;
            }

            return evaluationResult;
        }

        #endregion IEvaluation

        /// <summary>
        /// Parameter evaluator called off by function EvaluateExpression so as to elimiate the need of trying to cast the expression parameters to a more defined type.
        /// Additionally it adds the possibility that an expression parameter itself can be of complex type.
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="args">ParameterArgs object where to store the evaluation result</param>
        private void EvaluateParam(string name, ParameterArgs args)
        {
            if (this._definesHandler.SubstitutionTokens.ContainsKey(name))
            {
                object substituionText;
                this._definesHandler.SubstitutionTokens.TryGetValue(name, out substituionText);
                Expression parameterExpression = new Expression((string)substituionText);
                parameterExpression.EvaluateParameter += EvaluateParam;

                try
                {
                    args.Result = parameterExpression.Evaluate();
                }
                catch (EvaluationException)
                {
                    //in case an exception occurred we do not set args and hence the evaluation will be undefined
                }
            }
            //in case the key is not found we do not set args and hence the evaluation will be undefined
        }

        /// <summary>
        /// NCalc custom function evaluator
        /// </summary>
        /// <param name="name">name of the function to be evaluated</param>
        /// <param name="args">FunctionArgs object from where the function arguments will be read and the function evaluation result will be stored</param>
        private void EvaluateFunction(string name, FunctionArgs args)
        {
            if (name == "defined")
            {
                //it is here assumed that defined always takes one argument and is of type identifier
                Identifier identifier = (Identifier)args.Parameters[0].ParsedExpression;
                args.Result = _definesHandler.IsDefined(identifier.Name);
            }
        }
    }
}