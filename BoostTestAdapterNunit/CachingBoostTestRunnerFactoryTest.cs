// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;

using BoostTestAdapter.Boost.Runner;

using FakeItEasy;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class CachingBoostTestRunnerFactoryTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var stub = A.Fake<IBoostTestRunnerFactory>();

            A.CallTo(() => stub.GetRunner(A<string>._, A<BoostTestRunnerFactoryOptions>._)).ReturnsLazily((string identifier, BoostTestRunnerFactoryOptions options) =>
            {
                var runner = A.Fake<IBoostTestRunner>();
                A.CallTo(() => runner.Source).Returns(identifier);

                return runner;
            });

            Factory = new CachingBoostTestRunnerFactory(stub);
        }

        #endregion Test Setup/Teardown
        
        #region Test Data

        private CachingBoostTestRunnerFactory Factory { get; set; }
        
        #endregion Test Data

        #region Tests
        
        /// <summary>
        /// Assert that: Runners are cached as long as the factory options and the identifier are equivalent
        /// </summary>
        [Test]
        public void CacheRunnersBasedOnIdentifier()
        {
            var runner = Factory.GetRunner("hello", null);
            Assert.That(runner, Is.Not.Null);

            var runner2 = Factory.GetRunner("hello", null);
            Assert.That(runner2, Is.EqualTo(runner));
            
            var runner3 = Factory.GetRunner("not-hello", null);
            Assert.That(runner3, Is.Not.Null);
            Assert.That(runner3, Is.Not.EqualTo(runner2));
        }

        /// <summary>
        /// Assert that: Runners are cached as long as the factory options and the identifier are equivalent
        /// </summary>
        [Test]
        public void CacheRunnersBasedOnBoostVersion()
        {
            BoostTestRunnerFactoryOptions options = new BoostTestRunnerFactoryOptions
            {
                ForcedBoostTestVersion = DefaultBoostTestRunnerFactory.Boost159
            };

            var runner = Factory.GetRunner("hello", options);
            Assert.That(runner, Is.Not.Null);

            var runner2 = Factory.GetRunner("hello", options);
            Assert.That(runner2, Is.EqualTo(runner));

            var runner3 = Factory.GetRunner("not-hello", options);
            Assert.That(runner3, Is.Not.Null);
            Assert.That(runner3, Is.Not.EqualTo(runner2));
            
            var runner4 = Factory.GetRunner("hello", new BoostTestRunnerFactoryOptions());
            Assert.That(runner4, Is.Not.Null);
            Assert.That(runner4, Is.Not.EqualTo(runner2));
            Assert.That(runner4, Is.Not.EqualTo(runner3));
        }
        
        #endregion
    }
}
