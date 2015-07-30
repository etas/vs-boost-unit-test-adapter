using BoostTestAdapter.SourceFilter;
using NUnit.Framework;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class ExpressionEvaluationTest
    {
        /// <summary>
        /// Tests the expression evaluator with different expressions of varying complexities
        /// </summary>
        [TestCase("0", Result = EvaluationResult.IsFalse)]
        [TestCase("1", Result = EvaluationResult.IsTrue)]
        [TestCase("1+1", Result = EvaluationResult.IsTrue)]
        [TestCase("1+1-0", Result = EvaluationResult.IsTrue)]
        [TestCase("1+(1)", Result = EvaluationResult.IsTrue)]
        [TestCase("(1)",  Result = EvaluationResult.IsTrue)]
        [TestCase("1.0", Result = EvaluationResult.IsTrue)]
        [TestCase("VERSION > 0", "VERSION", "5", Result = EvaluationResult.IsTrue)]
        [TestCase("VERSION > 0", "VERSION", "0", Result = EvaluationResult.IsFalse)]
        [TestCase("0 || 2", Result = EvaluationResult.IsTrue)]
        [TestCase("1 && 2", Result = EvaluationResult.IsTrue)]
        [TestCase("0 && 1", Result = EvaluationResult.IsFalse)]
        [TestCase("VERSION > 0", Result = EvaluationResult.UnDetermined)]
        [TestCase("(1", Result = EvaluationResult.UnDetermined)]
        [TestCase("1/0", Result = EvaluationResult.IsTrue)]
        [TestCase("version > 0", "VERSION", "1", Result = EvaluationResult.UnDetermined)] /* case sensitivity test */
        [TestCase("VALUE1 > VALUE2", "VALUE1", "10", "VALUE2", "9", Result = EvaluationResult.IsTrue)]
        [TestCase("-1", Result = EvaluationResult.IsTrue)]
        [TestCase("BIG == 512", "BIG", "(512)", Result = EvaluationResult.IsTrue)]
        [TestCase("BIG == 512", "BIG", "(SMO + 1)", "SMO", "(511)", Result = EvaluationResult.IsTrue)]
        [TestCase("BIG == 512", "BIG", "(SMO + 1)", Result = EvaluationResult.UnDetermined)]
        [TestCase("BIGA == 512", Result = EvaluationResult.UnDetermined)]
        [TestCase("defined(TEST1) && !defined(TEST2)", Result = EvaluationResult.IsFalse)]
        [TestCase("defined(TEST1) && !defined(TEST2)", "TEST1", "", "TEST2", "", Result = EvaluationResult.IsFalse)]
        [TestCase("defined(TEST1) && !defined(TEST2)", "TEST1", "", Result = EvaluationResult.IsTrue)]
        public EvaluationResult ExpressionEvaluation(string expression, params string[] definitions)
        {
            ExpressionEvaluation e = new ExpressionEvaluation();
            return e.EvaluateExpression(expression, GenerateDefines(definitions));
        }

        /// <summary>
        /// Given a parameter list of strings, generates a Defines structure
        /// where pairs of strings are treated as a definition and its value.
        /// </summary>
        /// <param name="definitions">The string pair definitions array from which to generate the Defines structue</param>
        /// <returns>A Defines structure built out of string pairs available in the definitions array</returns>
        private Defines GenerateDefines(string[] definitions)
        {
            Assert.That(definitions.Length % 2, Is.EqualTo(0));

            Defines definesHandler = new Defines();

            for (int i = 1; i < definitions.Length; i += 2)
            {
                definesHandler.Define(definitions[i - 1], definitions[i]);
            }

            return definesHandler;
        }
    }
}
