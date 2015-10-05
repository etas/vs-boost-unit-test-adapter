// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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