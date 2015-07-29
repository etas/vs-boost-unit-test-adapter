using System.Collections.Generic;
using BoostTestAdapter;
using BoostTestAdapter.Utility.VisualStudio;
using BoostTestAdapterNunit.Fakes;
using FakeItEasy;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    internal class BoostTestDiscovererTest
    {
        /// <summary>
        /// The scope of this test is to check that if the Discoverer is given multiple project,
        /// method DiscoverTests splits appropiately the sources of type exe and of type dll in exe sources and dll sources
        /// and dispatches the discovery accordingly.
        /// </summary>
        [Test]
        public void CorrectBoostTestDiscovererDispatching()
        {
            var bootTestDiscovererFactory = A.Fake<IBoostTestDiscovererFactory>();
            var boostDllTestDiscoverer = A.Fake<IBoostTestDiscoverer>();
            var boostExeTestDiscoverer = A.Fake<IBoostTestDiscoverer>();
            var defaultTestContext = new DefaultTestContext();
            var consoleMessageLogger = new ConsoleMessageLogger();
            var defaultTestCaseDiscoverySink = new DefaultTestCaseDiscoverySink();

            ITestDiscoverer boostTestDiscoverer = new BoostTestDiscoverer(bootTestDiscovererFactory);

            var projects = new string[]
            {
                "project1" + BoostTestDiscoverer.DllExtension,
                "project2" + BoostTestDiscoverer.ExeExtension,
                "project3" + BoostTestDiscoverer.ExeExtension,
                "project4" + BoostTestDiscoverer.DllExtension,
                "project5" + BoostTestDiscoverer.DllExtension,
            };

            var dllProjectsExpected = new string[]
            {
                "project1" + BoostTestDiscoverer.DllExtension,
                "project4" + BoostTestDiscoverer.DllExtension,
                "project5" + BoostTestDiscoverer.DllExtension,
            };

            var exeProjectsExpected = new string[]
            {
                "project2" + BoostTestDiscoverer.ExeExtension,
                "project3" + BoostTestDiscoverer.ExeExtension,
            };

            IEnumerable<string> dllProjectsActual = null;
            IEnumerable<string> exeProjectsActual = null;

            A.CallTo(() => bootTestDiscovererFactory.GetTestDiscoverer(BoostTestDiscoverer.DllExtension, A<BoostTestDiscovererFactoryOptions>.Ignored))
                .Returns(boostDllTestDiscoverer);
            A.CallTo(() => bootTestDiscovererFactory.GetTestDiscoverer(BoostTestDiscoverer.ExeExtension, A<BoostTestDiscovererFactoryOptions>.Ignored))
                .Returns(boostExeTestDiscoverer);

            A.CallTo(
                    () =>
                        boostDllTestDiscoverer.DiscoverTests(A<IEnumerable<string>>.Ignored, defaultTestContext,
                            consoleMessageLogger, defaultTestCaseDiscoverySink))
                    .Invokes(call => dllProjectsActual = call.GetArgument<IEnumerable<string>>(0));

            A.CallTo(
                    () =>
                        boostExeTestDiscoverer.DiscoverTests(A<IEnumerable<string>>.Ignored, defaultTestContext,
                            consoleMessageLogger, defaultTestCaseDiscoverySink))
                    .Invokes(call => exeProjectsActual = call.GetArgument<IEnumerable<string>>(0));

            boostTestDiscoverer.DiscoverTests(projects, defaultTestContext, consoleMessageLogger, defaultTestCaseDiscoverySink);

            Assert.AreEqual(dllProjectsExpected, dllProjectsActual);
            Assert.AreEqual(exeProjectsExpected, exeProjectsActual);
        }
    }
}