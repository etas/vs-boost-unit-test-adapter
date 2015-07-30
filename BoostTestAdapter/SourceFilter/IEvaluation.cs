using VisualStudioAdapter;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Identifies the state of an evaluation result
    /// </summary>
    public enum EvaluationResult
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Un")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnDetermined")]
        UnDetermined = -1,
        IsFalse = 0,
        IsTrue
    }

    /// <summary>
    /// IEvaluation implementations evaluate boolean expressions.
    /// </summary>
    public interface IEvaluation
    {
        /// <summary>
        /// Given an expression encoded as a string, parses and evaluates the encoded boolean expression.
        /// </summary>
        /// <param name="expression">The string expression to evaluate</param>
        /// <param name="definesHandler">A collection of identifiers (named constants/variables) which may be referenced during evaluation</param>
        /// <returns>The result of the boolean expression evaluation. May return Undetermined if the statement cannot be parsed or evaluated.</returns>
        EvaluationResult EvaluateExpression(string expression, Defines definesHandler);
    }
}