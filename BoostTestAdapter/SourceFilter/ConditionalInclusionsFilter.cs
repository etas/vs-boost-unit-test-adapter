using BoostTestAdapter.SourceFilter.ConditionalInclusions;
using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// An ISourceFilter implementation which filters out source
    /// code based on preprocessor conditionals.
    /// </summary>
    public class ConditionalInclusionsFilter : ISourceFilter
    {
        #region Members

        private readonly ConditionalInclusionsMachine _conditionalInclusionsMachine;

        #endregion Members

        #region Constructor

        /// <summary>
        /// Constructor accepting an expression evaluator
        /// </summary>
        /// <param name="evaluator">expression evaluator</param>
        public ConditionalInclusionsFilter(IEvaluation evaluator)
        {
            _conditionalInclusionsMachine = new ConditionalInclusionsMachine(evaluator);
        }

        #endregion Constructor

        #region ISourceFilter

        /// <summary>
        /// Applies the filter action onto the source code
        /// </summary>
        /// <param name="cppSourceFile">source file information</param>
        /// <param name="definesHandler">pre-processor defines</param>
        public void Filter(CppSourceFile cppSourceFile, Defines definesHandler)
        {
            _conditionalInclusionsMachine.Apply(cppSourceFile, definesHandler);
        }

        #endregion ISourceFilter
    }
}