using System.Collections.Generic;
using BoostTestAdapter.SourceFilter;
using BoostTestAdapterNunit.Utility;
using NUnit.Framework;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class ConditionalInclusionsFilterTest
    {
        /// <summary>
        /// The scope of this test is to make sure of the proper handling of the if conditionals of the conditional inclusions filter. The filter is supplied with a fairly
        /// complex nested if structure and the result generated is compared against the expected filtered output.  For this test the expression complexity of the conditional
        /// is kept to a fairly low level.
        /// </summary>
        [Test]
        public void ConditionalInclusionsIfTests()
        {
            #region setup
            
            var definesHandler = new Defines();

            definesHandler.Define("DEBUG", "");
            definesHandler.Define("NDEBUG", "");
            definesHandler.Define("DEBUGGER", "");

            var expectedPreprocessorDefines = new HashSet<string>()
            {
                "DEBUG",
                "NDEBUG",
                "DEBUGGER",
                "DLEVEL"
            };

            var filter = new ConditionalInclusionsFilter( new ExpressionEvaluation() );

            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "ConditionalInclusionsIfTests_UnFilteredSourceCode.cpp";
            const string filteredSourceCodeResourceName = "ConditionalInclusionsIfTests_FilteredSourceCode.cpp";

            string sourceCodeOriginal = TestHelper.ReadEmbeddedResource(nameSpace + unfilteredSourceCodeResourceName);
            string sourceCodeExpected = TestHelper.ReadEmbeddedResource(nameSpace + filteredSourceCodeResourceName);

            var cppSourceFile = new CppSourceFile()
            {
                FileName = nameSpace + unfilteredSourceCodeResourceName,
                SourceCode = sourceCodeOriginal
            };

            #endregion

            #region excercise

            filter.Filter(cppSourceFile, definesHandler);

            #endregion

            #region verify

            Assert.AreEqual(sourceCodeExpected, cppSourceFile.SourceCode);
            Assert.AreEqual(expectedPreprocessorDefines, definesHandler.NonSubstitutionTokens);
            Assert.AreEqual(0, definesHandler.SubstitutionTokens.Count);

            #endregion

        }

        /// <summary>
        /// The scope of this test is to test the proper handling of the #define, #undef, #if, #else, #elif and #endif...with particular focus on the #define.
        /// This test limits itself on checking that the ConditionalInclusionsFilter handles properly the presence of the tokens, so text substitutions and 
        /// and parametrized macro are out of scope.
        /// </summary>
        [Test]
        public void ConditionalInclusionsIfdefTests()
        {

            #region setup

            var definesHandler = new Defines();

            definesHandler.Define("DEBUG", "");
            definesHandler.Define("NDEBUG", "");
            definesHandler.Define("DEBUGGER", "");

            var expectedPreprocessorDefines = new HashSet<string>()
            {
                "NDEBUG",
                "DEBUGGER"
            };

            var filter = new ConditionalInclusionsFilter( new ExpressionEvaluation());

            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "ConditionalInclusionsIfdefTests_UnFilteredSourceCode.cpp";
            const string filteredSourceCodeResourceName = "ConditionalInclusionsIfdefTests_FilteredSourceCode.cpp";
            
            string sourceCodeOriginal = TestHelper.ReadEmbeddedResource(nameSpace + unfilteredSourceCodeResourceName);
            string sourceCodeExpected = TestHelper.ReadEmbeddedResource(nameSpace + filteredSourceCodeResourceName);

            var cppSourceFile = new CppSourceFile()
            {
                FileName = nameSpace + unfilteredSourceCodeResourceName,
                SourceCode = sourceCodeOriginal
            };

            #endregion

            #region excercise

            filter.Filter(cppSourceFile, definesHandler);

            #endregion

            #region verify

            Assert.AreEqual(sourceCodeExpected, cppSourceFile.SourceCode);
            Assert.AreEqual(expectedPreprocessorDefines, definesHandler.NonSubstitutionTokens);
            Assert.AreEqual(0, definesHandler.SubstitutionTokens.Count);

            #endregion

        }

        /// <summary>
        /// The scope of this test is to make sure that if the state machine does not return to the "normal state", we bypass all filtering
        /// done on the file and return the source code unfiltered
        /// </summary>
        [Test]
        public void ConditionalInclusionsBadSourceFileNesting()
        {
            #region setup
            
            var definesHandler = new Defines(); //no defines supplied

            var filter = new ConditionalInclusionsFilter(new ExpressionEvaluation());
            
            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "ConditionalInclusionsBadSourceFileNesting.cpp";
            
            string sourceCodeOriginal = TestHelper.ReadEmbeddedResource(nameSpace + unfilteredSourceCodeResourceName);
            string sourceCodeExpected = sourceCodeOriginal;

            var cppSourceFile = new CppSourceFile()
            {
                FileName = nameSpace + unfilteredSourceCodeResourceName,
                SourceCode = sourceCodeOriginal
            };

            #endregion

            #region excercise

            filter.Filter(cppSourceFile, definesHandler);

            #endregion

            #region verify

            Assert.AreEqual(sourceCodeExpected, cppSourceFile.SourceCode); //no filtering should be done due to missing #endif

            #endregion

        }

        /// <summary>
        /// The scope of this test is to supply the filter with expressions that is not able to handle, so as to make sure
        /// that in case the filter is not able to handle the expression complexity we abort any filtering on the entire sourcecode
        /// and return back the source code unfiltered
        /// </summary>
        [Test]
        public void ConditionalInclusionsComplexExpressionEvaluationFail()
        {
            #region setup

            var definesHandler = new Defines(); //no defines supplied

            var filter = new ConditionalInclusionsFilter(new ExpressionEvaluation());

            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "ConditionalInclusionsComplexEvaluationFail.cpp";
            
            string sourceCodeOriginal = TestHelper.ReadEmbeddedResource(nameSpace + unfilteredSourceCodeResourceName);
            string sourceCodeExpected = sourceCodeOriginal;

            var cppSourceFile = new CppSourceFile()
            {
                FileName = nameSpace + unfilteredSourceCodeResourceName,
                SourceCode = sourceCodeOriginal
            };

            #endregion

            #region excercise

            filter.Filter(cppSourceFile, definesHandler);

            #endregion

            #region verify

            Assert.AreEqual(sourceCodeExpected, cppSourceFile.SourceCode); //no filtering should be done due to inability to evaluate an expression

            #endregion
        }

        /// <summary>
        /// The scope of this test is to supply the filter with expressions that it can handle and to make sure that it filters in and
        /// out the source code in a correct fashion.
        /// </summary>
        [Test]
        public void ConditionalInclusionsComplexExpressionEvaluationSuccess()
        {
            #region setup

            var definesHandler = new Defines();

            var expectedNonSubstitutionTokens = new HashSet<string>()
            {
                "VERSION",
                "HALF",
                "THIRD",
                "DEBUG",
                "SIN",
                "MAX",
                "CUBE",
                "fPRINT",
                "ASSERT",
            };

            var expectedSubtitutionTokens = new Dictionary<string, object>()
            {
                {"LEVEL", "19"},
                {"EVER", ";;"},
                {"BIG", "(512)"},
                {"PRINT", "cout << #x"},
            };

            var filter = new ConditionalInclusionsFilter(new ExpressionEvaluation());

            const string nameSpace = "BoostTestAdapterNunit.Resources.SourceFiltering.";
            const string unfilteredSourceCodeResourceName = "ConditionalInclusionsComplexEvaluationSuccess_UnfilteredSourceCode.cpp";
            const string filteredSourceCodeResourceName = "ConditionalInclusionsComplexEvaluationSuccess_FilteredSourceCode.cpp";
            
            string sourceCodeOriginal = TestHelper.ReadEmbeddedResource(nameSpace + unfilteredSourceCodeResourceName);

            string sourceCodeExpected = TestHelper.ReadEmbeddedResource(nameSpace + filteredSourceCodeResourceName);

            var cppSourceFile = new CppSourceFile()
            {
                FileName = nameSpace + unfilteredSourceCodeResourceName,
                SourceCode = sourceCodeOriginal
            };

            #endregion

            #region excercise

            filter.Filter(cppSourceFile, definesHandler);

            #endregion

            #region verify

            Assert.AreEqual(sourceCodeExpected, cppSourceFile.SourceCode);
            Assert.AreEqual(expectedNonSubstitutionTokens, definesHandler.NonSubstitutionTokens);
            Assert.AreEqual(expectedSubtitutionTokens, definesHandler.SubstitutionTokens);

            #endregion
        }

    }
}
