namespace BoostTestAdapter.SourceFilter.ConditionalInclusions
{
    /// <summary>
    /// Interface for any of the expected states of the conditional inclusions machine.
    ///
    ///     The different states expected are:
    ///        NormalState,
    ///        IncludingDueToIf,
    ///        DiscardingDueToSuccessfullIf,
    ///        DiscardingDueToFailedIf,
    ///        DiscardingDueToMultilineDefine
    ///
    /// </summary>
    internal interface IConditionalInclusionsState
    {
        void Process(ref string sourceLine, SourceLineType sourceLineType, string expression, string subtitutionText);
    }
}