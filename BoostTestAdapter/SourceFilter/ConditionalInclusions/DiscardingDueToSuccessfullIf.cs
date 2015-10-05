// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace BoostTestAdapter.SourceFilter.ConditionalInclusions
{
    /// <summary>
    /// Class handling the state when the Conditional Inclusions Machine is discarding code due to a previously successfull if statement
    /// </summary>
    internal class DiscardingDueToSuccessfullIf : IConditionalInclusionsState
    {
        private readonly ConditionalInclusionsMachine _conditionalInclusionsMachine;

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conditionalInclusionsMachine">reference to the state machine</param>
        internal DiscardingDueToSuccessfullIf(ConditionalInclusionsMachine conditionalInclusionsMachine)
        {
            _conditionalInclusionsMachine = conditionalInclusionsMachine;
        }

        #endregion Constructor

        /// <summary>
        /// Called so as to make the necessary decision making whilst the state machine is discarding code due to a previously
        /// successful if statement (i.e. we are currently in some elif or else statement)
        /// </summary>
        /// <param name="sourceLine">source line under analysis</param>
        /// <param name="sourceLineType">source code type as inspected by the Inspect function in the conditional inclusions machine</param>
        /// <param name="expression">not used</param>
        /// <param name="subtitutionText">not used</param>
        public void Process(ref string sourceLine, SourceLineType sourceLineType, string expression, string subtitutionText)
        {
            sourceLine = "";
            switch (sourceLineType)
            {
                case SourceLineType.Ifclause:
                case SourceLineType.Ifndefclause:
                case SourceLineType.Ifdefclause:
                    /*
                 * In case we encounter an other if whilst discarding we add an other layer
                 * so as to keep the matching endif clauses
                 */
                    _conditionalInclusionsMachine.AddState(ParserState.DiscardingDueToSuccessfullIf);
                    break;

                case SourceLineType.Endifclause:
                    _conditionalInclusionsMachine.FinishWithCurrentState();
                    break;
            }
        }
    }
}