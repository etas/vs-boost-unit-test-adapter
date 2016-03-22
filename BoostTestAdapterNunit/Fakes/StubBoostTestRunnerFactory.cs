// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Collections.Generic;

using BoostTestAdapter.Boost.Runner;

using FakeItEasy;

namespace BoostTestAdapterNunit.Fakes
{
    /// <summary>
    /// A stub IBoostTestRunnerFactory implementation
    /// </summary>
    public class StubBoostTestRunnerFactory : IBoostTestRunnerFactory
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StubBoostTestRunnerFactory()
            : this(Enumerable.Empty<string>())
        {
        }

        /// <summary>
        /// Constructor. Defines which test sources have list_content support.
        /// </summary>
        /// <param name="listContent">Enumeration of test source file paths which support list_content</param>
        public StubBoostTestRunnerFactory(IEnumerable<string> listContent)
        {
            this.ListContentSupport = listContent;
        }

        /// <summary>
        /// Enumeration of test source file paths which support list_content
        /// </summary>
        public IEnumerable<string> ListContentSupport { get; private set; }

        #region IBoostTestRunnerFactory

        public IBoostTestRunner GetRunner(string identifier, BoostTestRunnerFactoryOptions options)
        {
            IBoostTestRunner runner = A.Fake<IBoostTestRunner>();

            A.CallTo(() => runner.Source).Returns(identifier);
            A.CallTo(() => runner.ListContentSupported).Returns(ListContentSupport.Contains(identifier));

            return runner;
        }

        #endregion IBoostTestRunnerFactory
    }
}
