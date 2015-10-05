// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class CommandEvaluatorTest
    {
        [SetUp]
        public void SetUp()
        {
            CommandEvaluator = new CommandEvaluator();
        }

        private CommandEvaluator CommandEvaluator { get; set; }

        /// <summary>
        /// Variable placeholder string substition
        /// 
        /// Test aims:
        ///     - Ensure that variable placholders are properly substituted using their pre-defined value
        /// </summary>
        /// <param name="input">The un-evaluated string which is to be evaluated</param>
        /// <param name="variables">A list of string pairs consisting of the variable placeholder label and its respective value</param>
        /// <returns>The evaluated string</returns>
        [TestCase("Hello {World}!", "World", "Welt", Result = "Hello Welt!")]
        [TestCase("Hello {World}!", "World", null, Result = "Hello null!")]
        [TestCase("C:\\ExternalTestRunner.exe --test \"{source}\" --discover", "source", "boost.test.dll", "timeout", "10", Result = "C:\\ExternalTestRunner.exe --test \"boost.test.dll\" --discover")]
        [TestCase("{source} \"--run_test={test}\" {boost-args}", "source", "test.boostd.exe", "test", "TestSuite/TestCase", "timeout", "10", "boost-args", "--log_level=all", Result = "test.boostd.exe \"--run_test=TestSuite/TestCase\" --log_level=all")]
        [TestCase("{$}", "$", "jQuery", Result = "jQuery")]
        [TestCase("{MyVariable} {Is} {Not} {Available}", "MyVariable", "Cikku", Result = "Cikku {Is} {Not} {Available}")]
        [TestCase("{A}{B}{A}", "A", "1", "B", "2", Result = "121")]
        public string Evaluate(string input, params string[] variables)
        {
            Assert.That(variables.Length % 2, Is.EqualTo(0));

            for (int i = 0; i < variables.Length; i += 2)
            {
                CommandEvaluator.SetVariable(variables[i], variables[i + 1]);
            }

            return CommandEvaluator.Evaluate(input).Result;
        }
    }
}
