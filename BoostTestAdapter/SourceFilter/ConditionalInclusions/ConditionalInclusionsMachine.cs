using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BoostTestAdapter.Utility;
using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter.ConditionalInclusions
{
    internal enum SourceLineType
    {
        Other = 1,
        Ifclause,
        Elifclause,
        Elseclause,
        Endifclause,
        Defineclause,
        MultiLineDefineclause,
        Undefineclause,
        Ifdefclause,
        Ifndefclause
    }

    internal enum ParserState
    {
        NormalState = 1,
        IncludingDueToIf,
        DiscardingDueToSuccessfullIf,
        DiscardingDueToFailedIf,
        DiscardingDueToMultilineDefine
    }

    /// <summary>
    /// Filters source code that is controlled via the preprocessor directives such as:
    /// #if, #elif, #else, #endif, #ifdef, #ifndef, #undef, #define, #if defined and if !defined
    /// </summary>
    public class ConditionalInclusionsMachine
    {
        private static readonly Regex LinefeedRegex = new Regex(@"[ \t]*?\r??\n", RegexOptions.Singleline | RegexOptions.Multiline);
        private static readonly Regex UndefineRegex = new Regex(@"#undef\s{1,}(\w{1,})");
        private static readonly Regex IfdefRegex = new Regex(@"#if(?:(?:\s{1,}defined)|(?:def))\s{1,}(\w{1,})");
        private static readonly Regex IfndefRegex = new Regex(@"#if(?:(?:\s{1,}!(?:\s{0,})defined)|(?:ndef))\s{1,}(\w{1,})");
        private static readonly Regex IfRegex = new Regex(@"#if\s+?(.+)");
        private static readonly Regex ElifRegex = new Regex(@"#elif\s+?(.+)");
        private static readonly Regex ElseRegex = new Regex(@"#else");
        private static readonly Regex EndifRegex = new Regex(@"#endif");
        private static readonly Regex DefineRegex = new Regex(@"#define\s+(\w{1,})(.*)");
        private static readonly Regex DefineRegexMacro = new Regex(@"#define\s{1,}(\w{1,})\(");
        private static readonly Regex DefineRegexMultiline = new Regex(@"#define\s{1,}(\w{1,}).*\\\s{0,}$", RegexOptions.Multiline);

        private readonly IConditionalInclusionsState _normalState;
        private readonly IConditionalInclusionsState _includingDueToIf;
        private readonly IConditionalInclusionsState _discardingDueToSuccessfullIf;
        private readonly IConditionalInclusionsState _discardingDueToFailedIf;
        private readonly IConditionalInclusionsState _discardingDueToMultilineDefine;

        private IConditionalInclusionsState _conditionalInclusionsMachineState;

        private readonly Stack<ParserState> _parserState;

        /// <summary>
        ///
        /// </summary>
        internal IEvaluation Evaluator { get; private set; }

        internal Defines DefinesHandler { get; private set; }

        #region Constructors

        /// <summary>
        /// Constructor accepting an expression evaluator object
        /// </summary>
        public ConditionalInclusionsMachine(IEvaluation evaluator)
        {
            this.Evaluator = evaluator;
            this._parserState = new Stack<ParserState>();

            _normalState = new NormalState(this);
            _includingDueToIf = new IncludingDueToIf(this);
            _discardingDueToSuccessfullIf = new DiscardingDueToSuccessfullIf(this);
            _discardingDueToFailedIf = new DiscardingDueToFailedIf(this);
            _discardingDueToMultilineDefine = new DiscardingDueToMultilineDefine(this);
        }

        #endregion Constructors

        /// <summary>
        /// Updates the state machine according to the state indicated by the top most element of the stack
        /// </summary>
        private void UpdateStateMachine()
        {
            switch (_parserState.Peek())
            {
                case ParserState.NormalState:
                    _conditionalInclusionsMachineState = _normalState;
                    break;

                case ParserState.IncludingDueToIf:
                    _conditionalInclusionsMachineState = _includingDueToIf;
                    break;

                case ParserState.DiscardingDueToSuccessfullIf:
                    _conditionalInclusionsMachineState = _discardingDueToSuccessfullIf;
                    break;

                case ParserState.DiscardingDueToFailedIf:
                    _conditionalInclusionsMachineState = _discardingDueToFailedIf;
                    break;

                case ParserState.DiscardingDueToMultilineDefine:
                    _conditionalInclusionsMachineState = _discardingDueToMultilineDefine;
                    break;

                default:
                    throw new SourceDiscoveryException("Programming error. Found un-catered machine state (" + _parserState.Peek() + ")");
            }
        }

        /// <summary>
        /// Adds a new state to the stack and updates the state machine according to the new state
        /// </summary>
        /// <param name="newParserState">new state</param>
        internal void AddState(ParserState newParserState)
        {
            _parserState.Push(newParserState);
            UpdateStateMachine();
        }

        /// <summary>
        /// Updates the current state of the state machine with a newly defined state.
        /// </summary>
        /// <param name="newParserState">new state</param>
        internal void UpdateCurrentState(ParserState newParserState)
        {
            _parserState.Pop();
            _parserState.Push(newParserState);
            UpdateStateMachine();
        }

        /// <summary>
        /// Called whenever we are done with the current state and therefore we can continue with
        /// the previous state before wed branched off in handling another state
        /// </summary>
        internal void FinishWithCurrentState()
        {
            _parserState.Pop();
            UpdateStateMachine();
        }

        /// <summary>
        /// Driver method for the management of the state machine
        /// </summary>
        /// <param name="cppSourceFile">source file information</param>
        /// <param name="definesHandler">pre-processor defines</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "cpp")]
        public void Apply(CppSourceFile cppSourceFile, Defines definesHandler)
        {
            Utility.Code.Require(cppSourceFile, "cppSourceFile");

            DefinesHandler = definesHandler;

            string[] sourceLines = LinefeedRegex.Split(cppSourceFile.SourceCode);

            _parserState.Clear();
            AddState(ParserState.NormalState);  //initial state

            SourceLineType sourceLineType;
            string expression;
            string subtitutionText;
            int lineNumber = 0;

            try
            {
                for (; lineNumber < sourceLines.Length; lineNumber++)
                {
                    Inspect(sourceLines[lineNumber], out sourceLineType, out expression, out subtitutionText);

                    _conditionalInclusionsMachineState.Process(ref sourceLines[lineNumber], sourceLineType, expression,
                        subtitutionText);
                }

                /*
                 * Once the parsing is complete we just check that the parserState is back to Normal State.
                 * If not it is either because we parsed bad code (i.e. the code structure was not consistent to start with)
                 * or we've got a problem (programmatically) with our state engine
                 */

                //sanity check
                if (_parserState.Peek() != ParserState.NormalState)
                {
                    Logger.Error("The conditionals filter state machine failed to return to normal state. The source file \"{0}\" will not be filtered for conditionals.", cppSourceFile.FileName);

                    //the source code for the specific file is left unfiltered in case the state machine did not return to a normal state.
                }
                else
                {
                    //State machine returned to normal state so we can apply the filtering done by the conditional inclusions machine
                    cppSourceFile.SourceCode = string.Join(Environment.NewLine, sourceLines);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                        "The conditionals filter encountered error: {0} whilst processing line number {1} of source file \"{2}\". The source file will not be filtered for conditionals",
                        ex.Message, lineNumber + 1, cppSourceFile.FileName);
            }
        }

        /// <summary>
        /// Inspects the source line and sets the sourceLineType (byref) according to detected code syntax
        /// and sets also the expression variable (byref) to the matched identifier statement/expression where applicable
        /// </summary>
        /// <param name="sourceLine">source line to inspect</param>
        /// <param name="sourceLineType">match type</param>
        /// <param name="token">extracted expression or identifier respective to the source type</param>
        /// <param name="substitutiontext">extracted substitution text (applicable only to a define match)</param>
        private static void Inspect(string sourceLine, out SourceLineType sourceLineType, out string token, out string substitutiontext)
        {
            if (ElifRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Elifclause;
                token = ElifRegex.Match(sourceLine).Groups[1].Value;
                substitutiontext = "";
            }
            else if (ElseRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Elseclause;
                token = "";
                substitutiontext = "";
            }
            else if (EndifRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Endifclause;
                token = "";
                substitutiontext = "";
            }
            else if (IfdefRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Ifdefclause;
                token = IfdefRegex.Match(sourceLine).Groups[1].Value;
                substitutiontext = "";
            }
            else if (IfndefRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Ifndefclause;
                token = IfndefRegex.Match(sourceLine).Groups[1].Value;
                substitutiontext = "";
            }
            else if (IfRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Ifclause;
                token = IfRegex.Match(sourceLine).Groups[1].Value;
                substitutiontext = "";
            }
            else if (DefineRegexMultiline.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.MultiLineDefineclause;
                token = DefineRegexMultiline.Match(sourceLine).Groups[1].Value;
                substitutiontext = "";
            }
            else if (DefineRegexMacro.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Defineclause;
                token = DefineRegexMacro.Match(sourceLine).Groups[1].Value;
                substitutiontext = "";
            }
            else if (DefineRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Defineclause;
                token = DefineRegex.Match(sourceLine).Groups[1].Value;
                substitutiontext = DefineRegex.Match(sourceLine).Groups[2].Value.Trim();
            }
            else if (UndefineRegex.IsMatch(sourceLine))
            {
                sourceLineType = SourceLineType.Undefineclause;
                token = UndefineRegex.Match(sourceLine).Groups[1].Value;
                substitutiontext = "";
            }
            else
            {
                //any other type of source code that is an unrelated to the conditional inclusions
                sourceLineType = SourceLineType.Other;
                token = "";
                substitutiontext = "";
            }
        }
    }
}