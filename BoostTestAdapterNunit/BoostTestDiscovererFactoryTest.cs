// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using BoostTestAdapter;
using BoostTestAdapter.Discoverers;
using BoostTestAdapter.Settings;
using BoostTestAdapterNunit.Fakes;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestDiscovererFactoryTest
    {

        #region Tests

        /// <summary>
        /// The aim of this test is to check that if the Discoverer is given multiple projects,
        /// the factory dispatches the discoverers accordingly to the type of source.
        /// In this test the external runner is not configured so the two DLL projects should be ignored.
        /// </summary>
        [Test]
        public void CorrectMultipleBoostTestDiscovererDispatchingNoExternal()
        {
            var sources = new[]
            {
                "ListContentSupport" + BoostTestDiscoverer.ExeExtension,
                "ParseSources1" + BoostTestDiscoverer.ExeExtension,
                "ParseSources2" + BoostTestDiscoverer.ExeExtension,
                "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension,
            };

            var stubListContentHelper = new StubListContentHelper();
            var boostTestDiscovererFactory = new BoostTestDiscovererFactory(stubListContentHelper);

            var results = boostTestDiscovererFactory.GetDiscoverers(sources, new BoostTestAdapterSettings());

            Assert.That(results.Count(), Is.EqualTo(2));
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ListContentDiscoverer), Is.Not.Null);
            var lcd = results.First(x => x.Discoverer is ListContentDiscoverer);
            Assert.That(lcd.Sources, Is.EqualTo(new[] { "ListContentSupport" + BoostTestDiscoverer.ExeExtension }));

            Assert.That(results.FirstOrDefault(x => x.Discoverer is SourceCodeDiscoverer), Is.Not.Null);
            var scd = results.First(x => x.Discoverer is SourceCodeDiscoverer);
            Assert.That(scd.Sources, Is.EqualTo(new[] { "ParseSources1" + BoostTestDiscoverer.ExeExtension,
                "ParseSources2" + BoostTestDiscoverer.ExeExtension }));

            Assert.That(results.FirstOrDefault(x => x.Sources.Contains("DllProject1" + BoostTestDiscoverer.DllExtension)), Is.Null);
            Assert.That(results.FirstOrDefault(x => x.Sources.Contains("DllProject2" + BoostTestDiscoverer.DllExtension)), Is.Null);

        }

        /// <summary>
        /// The aim of this test is to check that if the Discoverer is given multiple projects,
        /// the factory dispatches the discoverers accordingly to the type of source.
        /// In this test the two DLL projects should be assigned to the ExternalDiscoverer that supports the DLL extension
        /// </summary>
        [Test]
        public void CorrectMultipleBoostTestDiscovererDispatchingWithExternalDll()
        {
            var sources = new[]
            {
                "ListContentSupport" + BoostTestDiscoverer.ExeExtension,
                "ParseSources1" + BoostTestDiscoverer.ExeExtension,
                "ParseSources2" + BoostTestDiscoverer.ExeExtension,
                "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension,
            };

            var stubListContentHelper = new StubListContentHelper();
            var boostTestDiscovererFactory = new BoostTestDiscovererFactory(stubListContentHelper);
            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();
            settings.ExternalTestRunner = new ExternalBoostTestRunnerSettings
            {
                ExtensionType = BoostTestDiscoverer.DllExtension
            };

            var results = boostTestDiscovererFactory.GetDiscoverers(sources, settings);

            Assert.That(results.Count(), Is.EqualTo(3));
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ListContentDiscoverer), Is.Not.Null);
            var lcd = results.First(x => x.Discoverer is ListContentDiscoverer);
            Assert.That(lcd.Sources, Is.EqualTo(new[] { "ListContentSupport" + BoostTestDiscoverer.ExeExtension }));

            Assert.That(results.FirstOrDefault(x => x.Discoverer is SourceCodeDiscoverer), Is.Not.Null);
            var scd = results.First(x => x.Discoverer is SourceCodeDiscoverer);
            Assert.That(scd.Sources, Is.EqualTo(new[] { "ParseSources1" + BoostTestDiscoverer.ExeExtension,
                "ParseSources2" + BoostTestDiscoverer.ExeExtension }));

            Assert.That(results.FirstOrDefault(x => x.Discoverer is ExternalDiscoverer), Is.Not.Null);
            var exd = results.First(x => x.Discoverer is ExternalDiscoverer);
            Assert.That(exd.Sources, Is.EqualTo(new[] { "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension }));
        }


        /// <summary>
        /// The aim of this test is to check that if the Discoverer is given multiple projects,
        /// the factory dispatches the discoverers accordingly to the type of source.
        /// In this test the ExternalDiscoverer supports the EXE extension so the DLL projects should be ignored and 
        /// all the other projects should be assigned to the ExternalDiscoverer.
        /// </summary>
        [Test]
        public void CorrectMultipleBoostTestDiscovererDispatchingWithExternalExe()
        {
            var sources = new[]
            {
                "ListContentSupport" + BoostTestDiscoverer.ExeExtension,
                "ParseSources1" + BoostTestDiscoverer.ExeExtension,
                "ParseSources2" + BoostTestDiscoverer.ExeExtension,
                "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension,
            };

            var stubListContentHelper = new StubListContentHelper();
            var boostTestDiscovererFactory = new BoostTestDiscovererFactory(stubListContentHelper);
            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();
            settings.ExternalTestRunner = new ExternalBoostTestRunnerSettings
            {
                ExtensionType = BoostTestDiscoverer.ExeExtension
            };

            var results = boostTestDiscovererFactory.GetDiscoverers(sources, settings);

            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ListContentDiscoverer), Is.Null);
            Assert.That(results.FirstOrDefault(x => x.Discoverer is SourceCodeDiscoverer), Is.Null);
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ExternalDiscoverer), Is.Not.Null);

            var exd = results.First(x => x.Discoverer is ExternalDiscoverer);
            Assert.That(exd.Sources, Is.EqualTo(new[] { "ListContentSupport" + BoostTestDiscoverer.ExeExtension,
                "ParseSources1" + BoostTestDiscoverer.ExeExtension,
                "ParseSources2" + BoostTestDiscoverer.ExeExtension, }));
        }

        /// <summary>
        /// The aim of this test is to check that if the Discoverer is given a single project,
        /// the factory returns the discoverer accordingly to the type of source.
        /// In this test the ExternalDiscoverer supports the EXE extension so the factory
        /// should return an instance of ExternalDiscoverer for all the types of exe projects.
        /// No discoverer should be returned for a dll project.
        /// </summary>
        [Test]
        public void CorrectSingleProjectBoostTestDiscovererDispatchingExternalExe()
        {
            var stubListContentHelper = new StubListContentHelper();
            var boostTestDiscovererFactory = new BoostTestDiscovererFactory(stubListContentHelper);
            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();
            settings.ExternalTestRunner = new ExternalBoostTestRunnerSettings
            {
                ExtensionType = BoostTestDiscoverer.ExeExtension
            };

            // source that supports --list-content parameter
            var source = "ListContentSupport" + BoostTestDiscoverer.ExeExtension;
            var discoverer = boostTestDiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ExternalDiscoverer)));

            // source that NOT supports --list-content parameter
            source = "ParseSources" + BoostTestDiscoverer.ExeExtension;
            discoverer = boostTestDiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ExternalDiscoverer)));

            // source dll project
            source = "DllProject" + BoostTestDiscoverer.DllExtension;
            discoverer = boostTestDiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Null);

        }


        /// <summary>
        /// The aim of this test is to check that if the Discoverer is given a single project,
        /// the factory returns the discoverer accordingly to the type of source.
        /// In this test the ExternalDiscoverer supports the DLL extension so the factory
        /// should return the correct type of discoverer according with the type of source.
        /// </summary>
        [Test]
        public void CorrectSingleProjectBoostTestDiscovererDispatchingExternalDll()
        {
            var stubListContentHelper = new StubListContentHelper();
            var boostTestDiscovererFactory = new BoostTestDiscovererFactory(stubListContentHelper);
            BoostTestAdapterSettings settings = new BoostTestAdapterSettings();
            settings.ExternalTestRunner = new ExternalBoostTestRunnerSettings
            {
                ExtensionType = BoostTestDiscoverer.DllExtension
            };

            // source that supports --list-content parameter
            var source = "ListContentSupport" + BoostTestDiscoverer.ExeExtension;
            var discoverer = boostTestDiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ListContentDiscoverer)));

            // source that NOT supports --list-content parameter
            source = "ParseSources" + BoostTestDiscoverer.ExeExtension;
            discoverer = boostTestDiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(SourceCodeDiscoverer)));

            // source dll project
            source = "DllProject" + BoostTestDiscoverer.DllExtension;
            discoverer = boostTestDiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ExternalDiscoverer)));

        }

        #endregion
    }
}
