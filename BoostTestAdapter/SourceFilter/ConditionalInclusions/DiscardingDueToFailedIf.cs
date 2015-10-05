// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Globalization;

namespace BoostTestAdapter.SourceFilter.ConditionalInclusions
{
    /// <summary>
    /// Class handling the state when the Conditional Inclusions Machine is discarding code when an if statement evaluated to false
    /// </summary>
    internal class DiscardingDueToFailedIf : IConditionalInclusionsState
    {
        private readonly ConditionalInclusionsMachine _conditionalInclusionsMachine;

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conditionalInclusionsMachine">reference to the state machine</param>
        internal DiscardingDueToFailedIf(ConditionalInclusionsMachine conditionalInclusionsMachine)
        {
            _conditionalInclusionsMachine = conditionalInclusionsMachine;
        }

        #endregion Constructor

        /// <summary>
        /// Called so as to make the necessary decision making whilst the state machine is discarding code during a failed if
        /// </summary>
        /// <param name="sourceLine">source line under analysis</param>
        /// <param name="sourceLineType">source code type as inspected by the Inspect function in the conditional inclusions machine</param>
        /// <param name="expression">expression to be evaluated in case the source Line type is an elif</param>
        /// <param name="subtitutionText"></param>
        public void Process(ref string sourceLine, SourceLineType sourceLineType, string expression, string subtitutionText)
        {
            sourceLine = "";
            switch (sourceLineType)
            {
                case SourceLineType.Ifclause:
                case SourceLineType.Ifdefclause:
                case SourceLineType.Ifndefclause:
                    /*
                 * In case we encounter an other if whilst discarding we add an other layer
                 * so as to keep the matching endif clauses.
                 *
                 * The reason why we push DiscardingDueToSuccessfullIf is to suppress any
                 * attempt any elif and else at this level.
                 */
                    _conditionalInclusionsMachine.AddState(ParserState.DiscardingDueToSuccessfullIf);
                    break;

                case SourceLineType.Elifclause:
                    switch (_conditionalInclusionsMachine.Evaluator.EvaluateExpression(expression, _conditionalInclusionsMachine.DefinesHandler))
                    {
                        case EvaluationResult.IsFalse:
                            /*
                         * in case the evaluation failed the state remains the same. We just
                         * continue discarding.
                         */
                            break;

                        case EvaluationResult.IsTrue:
                            /*
                         * in case the evaluation is successfull we just change the state of
                         * the current level
                         */
                            _conditionalInclusionsMachine.UpdateCurrentState(ParserState.IncludingDueToIf);
                            break;

                        case EvaluationResult.UnDetermined:
                            throw new SourceDiscoveryException(string.Format(CultureInfo.InvariantCulture, "Unable to evaluate expression \"{0}\"", expression));
                    }
                    break;

                case SourceLineType.Elseclause:
                    _conditionalInclusionsMachine.UpdateCurrentState(ParserState.IncludingDueToIf);
                    break;

                case SourceLineType.Endifclause:
                    _conditionalInclusionsMachine.FinishWithCurrentState();
                    break;
            }
        }
    }
}