namespace BoostTestAdapter.Boost.Test
{
    /// <summary>
    /// Visitor design pattern interface for Boost.Test.TestUnit visitor implementations.
    /// </summary>
    public interface ITestVisitor
    {
        /// <summary>
        /// Visits the provided TestCase
        /// </summary>
        /// <param name="testCase">The TestCase which is to be visited</param>
        void Visit(TestCase testCase);

        /// <summary>
        /// Visits the provided TestSuite
        /// </summary>
        /// <param name="testSuite">The TestSuite which is to be visited</param>
        void Visit(TestSuite testSuite);
    }
}