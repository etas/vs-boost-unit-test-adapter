// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoostTestAdapter;
using BoostTestAdapter.Discoverers;
using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility.VisualStudio;
using BoostTestAdapterNunit.Fakes;
using BoostTestAdapterNunit.Utility;
using NUnit.Framework;
using ListContentHelper = BoostTestAdapterNunit.Fakes.StubListContentHelper;

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
        public void Sink_ShouldContainTestForAllSupportedTypeOfSources()
        {
            var sources = new[]
            {
                "ListContentSupport" + BoostTestDiscoverer.ExeExtension,
                "ParseSources1" + BoostTestDiscoverer.ExeExtension,
                "ParseSources2" + BoostTestDiscoverer.ExeExtension,
                "DllProject1" + BoostTestDiscoverer.DllExtension,
                "DllProject2" + BoostTestDiscoverer.DllExtension,
            };

            var context = new DefaultTestContext();
            var logger = new ConsoleMessageLogger();
            var sink = new DefaultTestCaseDiscoverySink();

            context.RegisterSettingProvider(BoostTestAdapterSettings.XmlRootName, new BoostTestAdapterSettingsProvider());
            context.LoadEmbeddedSettings("BoostTestAdapterNunit.Resources.Settings.externalTestRunner.runsettings");

            var boostTestDiscovererFactory = new StubBoostTestDiscovereFactory();

            var boostTestDiscoverer = new BoostTestDiscoverer(boostTestDiscovererFactory);
            boostTestDiscoverer.DiscoverTests(sources, context, logger, sink);

            Assert.That(sink.Tests, Is.Not.Empty);

            // tests are found in the using the fake debughelper
            Assert.That(sink.Tests.Count(x => x.Source == "ListContentSupport" + BoostTestDiscoverer.ExeExtension), Is.EqualTo(7));

            // tests are found in the fake solution 
            Assert.That(sink.Tests.Count(x => x.Source == "ParseSources1" + BoostTestDiscoverer.ExeExtension), Is.EqualTo(6));

            // no (fake) solution code is found for this project
            Assert.That(sink.Tests.Any(x => x.Source == "ParseSources2" + BoostTestDiscoverer.ExeExtension), Is.False);
            
            // the external runner does NOT support the two dll projects
            Assert.That(sink.Tests.Any(x => x.Source == "DllProject1" + BoostTestDiscoverer.DllExtension), Is.False);
            Assert.That(sink.Tests.Any(x => x.Source == "DllProject2" + BoostTestDiscoverer.DllExtension), Is.False);
        }
    }

    internal class StubBoostTestDiscovereFactory : IBoostTestDiscovererFactory, IDisposable
    {
        private readonly DummySolution _dummySolution = new DummySolution("ParseSources1" + BoostTestDiscoverer.ExeExtension, "BoostUnitTestSample.cpp");

        public IBoostTestDiscoverer GetDiscoverer(string source, BoostTestAdapterSettings options)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FactoryResult> GetDiscoverers(IReadOnlyCollection<string> sources, BoostTestAdapterSettings settings)
        {
            var tmpSources = new List<string>(sources);
            var discoverers = new List<FactoryResult>();

            // sources that can be run on the external runner
            if (settings.ExternalTestRunner != null)
            {
                var extSources = tmpSources
                    .Where(s => settings.ExternalTestRunner.ExtensionType == Path.GetExtension(s))
                    .ToList();

                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new ExternalDiscoverer(settings.ExternalTestRunner),
                    Sources = extSources
                });

                tmpSources.RemoveAll(s => extSources.Contains(s));
            }

            // sources that support list-content parameter
            var listContentHelper = new StubListContentHelper();

            var listContentSources = tmpSources
                .Where(s => Path.GetExtension(s) == BoostTestDiscoverer.ExeExtension)
                .Where(listContentHelper.IsListContentSupported)
                .ToList();

            if (listContentSources.Count > 0)
            {
                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new ListContentDiscoverer(listContentHelper),
                    Sources = listContentSources
                });

                tmpSources.RemoveAll(s => listContentSources.Contains(s));
            }


            // sources that NOT support the list-content parameter
            var sourceCodeSources = tmpSources
                .Where(s => Path.GetExtension(s) == BoostTestDiscoverer.ExeExtension)
                .ToList();

            // TO-DO: setup the fake environment

            if (sourceCodeSources.Count > 0)
            {
                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new SourceCodeDiscoverer(_dummySolution.Provider),
                    Sources = sourceCodeSources
                });
            }
            return discoverers;

        }

        public void Dispose()
        {
            ((IDisposable)_dummySolution).Dispose();
        }
    }
}