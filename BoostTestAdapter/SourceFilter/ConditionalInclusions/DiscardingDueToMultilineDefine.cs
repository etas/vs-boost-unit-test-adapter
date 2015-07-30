using System.Text.RegularExpressions;

namespace BoostTestAdapter.SourceFilter.ConditionalInclusions
{
    /// <summary>
    /// Class handling the state when the Conditional Inclusions Machine is discarding code due to a multiline define.
    /// </summary>
    /// <remarks>Multiline defines are not supported by the current implementation and so any substitution text is just being discarded</remarks>
    internal class DiscardingDueToMultilineDefine : IConditionalInclusionsState
    {
        private static readonly Regex defineRegexMultilineContinuation = new Regex(@"\\\s*$");
        private readonly ConditionalInclusionsMachine _conditionalInclusionsMachine;

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conditionalInclusionsMachine">reference to the state machine</param>
        internal DiscardingDueToMultilineDefine(ConditionalInclusionsMachine conditionalInclusionsMachine)
        {
            _conditionalInclusionsMachine = conditionalInclusionsMachine;
        }

        #endregion Constructor

        /// <summary>
        /// Called so as to make the necessary decision making whilst the state machine is discarding code during a multiline define
        /// </summary>
        /// <param name="sourceLine">source line under analysis</param>
        /// <param name="sourceLineType">source code type as inspected by the Inspect function in the conditional inclusions machine</param>
        /// <param name="expression">not used</param>
        /// <param name="subtitutionText">not used</param>
        public void Process(ref string sourceLine, SourceLineType sourceLineType, string expression, string subtitutionText)
        {
            if (!(defineRegexMultilineContinuation.IsMatch(sourceLine)))
            {
                //last line of the multiline define met (which should be discarded as well)
                _conditionalInclusionsMachine.FinishWithCurrentState();
            }
            {
                //still in the multiline define
            }
            sourceLine = "";
        }
    }
}