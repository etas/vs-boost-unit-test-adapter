// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Text.RegularExpressions;

using BoostTestAdapter;
using BoostTestAdapter.Discoverers;
using BoostTestAdapter.Settings;

using BoostTestAdapter.Boost.Runner;

using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;

using NUnit.Framework;
using FakeItEasy;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    class BoostTestDiscovererFactoryTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.RunnerFactory = new StubBoostTestRunnerFactory(new[] { ("ListContentSupport" + BoostTestDiscoverer.ExeExtension) });
            this.DiscovererFactory = new BoostTestDiscovererFactory(this.RunnerFactory, DummyVSProvider.Default);
        }

        #endregion Test Setup/Teardown

        /// <summary>
        /// Default factory function for adapter settings
        /// </summary>
        /// <returns>The default settings for use within this test fixture</returns>
        private static BoostTestAdapterSettings CreateAdapterSettings()
        {
            // Prefer the use of ListContent where possible
            return new BoostTestAdapterSettings();
        }

        private static ExternalBoostTestRunnerSettings CreateExternalRunnerSettings(string extension)
        {
            return new ExternalBoostTestRunnerSettings() { ExtensionType = new Regex(extension) };
        }
        
        #region Test Data

        private IBoostTestRunnerFactory RunnerFactory { get; set; }

        private IBoostTestDiscovererFactory DiscovererFactory { get; set; }

        #endregion Test Data

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
                "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension,
            };
            
            var results = this.DiscovererFactory.GetDiscoverers(sources, CreateAdapterSettings());

            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ListContentDiscoverer), Is.Not.Null);
            var lcd = results.First(x => x.Discoverer is ListContentDiscoverer);
            Assert.That(lcd.Sources, Is.EqualTo(new[] { "ListContentSupport" + BoostTestDiscoverer.ExeExtension }));

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
                "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension,
            };
            
            BoostTestAdapterSettings settings = CreateAdapterSettings();
            settings.ExternalTestRunner = CreateExternalRunnerSettings(BoostTestDiscoverer.DllExtension);

            var results = this.DiscovererFactory.GetDiscoverers(sources, settings);

            Assert.That(results.Count(), Is.EqualTo(2));
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ListContentDiscoverer), Is.Not.Null);
            var lcd = results.First(x => x.Discoverer is ListContentDiscoverer);
            Assert.That(lcd.Sources, Is.EqualTo(new[] { "ListContentSupport" + BoostTestDiscoverer.ExeExtension }));

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
                "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension,
            };
            
            BoostTestAdapterSettings settings = CreateAdapterSettings();
            settings.ExternalTestRunner = CreateExternalRunnerSettings(BoostTestDiscoverer.ExeExtension);

            var results = this.DiscovererFactory.GetDiscoverers(sources, settings);

            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ListContentDiscoverer), Is.Null);
            Assert.That(results.FirstOrDefault(x => x.Discoverer is ExternalDiscoverer), Is.Not.Null);

            var exd = results.First(x => x.Discoverer is ExternalDiscoverer);
            Assert.That(exd.Sources, Is.EqualTo(new[] { "ListContentSupport" + BoostTestDiscoverer.ExeExtension,
            }));
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
            BoostTestAdapterSettings settings = CreateAdapterSettings();
            settings.ExternalTestRunner = CreateExternalRunnerSettings(BoostTestDiscoverer.ExeExtension);

            // source that supports --list-content parameter
            var source = "ListContentSupport" + BoostTestDiscoverer.ExeExtension;
            var discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ExternalDiscoverer)));

            // source that NOT supports --list-content parameter
            source = "ParseSources" + BoostTestDiscoverer.ExeExtension;
            discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ExternalDiscoverer)));

            // source dll project
            source = "DllProject" + BoostTestDiscoverer.DllExtension;
            discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

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
            BoostTestAdapterSettings settings = CreateAdapterSettings();
            settings.ExternalTestRunner = CreateExternalRunnerSettings(BoostTestDiscoverer.DllExtension);

            // source that supports --list-content parameter
            var source = "ListContentSupport" + BoostTestDiscoverer.ExeExtension;
            var discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ListContentDiscoverer)));
 
            // source dll project
            source = "DllProject" + BoostTestDiscoverer.DllExtension;
            discoverer = this.DiscovererFactory.GetDiscoverer(source, settings);

            Assert.That(discoverer, Is.Not.Null);
            Assert.That(discoverer, Is.AssignableFrom(typeof(ExternalDiscoverer)));

        }

        #endregion
    }
}
