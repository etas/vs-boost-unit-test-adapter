using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoostTestAdapter.Discoverers;

namespace BoostTestAdapter
{
    class BoostTestDiscovererFactory : IBoostTestDiscovererFactory
    {
        #region Constructors

        public BoostTestDiscovererFactory()
            : this(new ListContentHelper())
        {

        }

        public BoostTestDiscovererFactory(IListContentHelper listContentHelper)
        {
            _listContentHelper = listContentHelper;
        }

        #endregion


        #region Members

        private readonly IListContentHelper _listContentHelper;

        #endregion


        #region IBoostTestDiscovererFactory

        public IBoostTestDiscoverer GetDiscoverer(string source, Settings.BoostTestAdapterSettings options)
        {
            var list = new List<string> { source };
            var results = GetDiscoverers(list, options);
            if (results != null)
            {
                var r = results.FirstOrDefault(x => x.Sources.Contains(source));
                if (r != null)
                    return r.Discoverer;
            }

            return null;
        }

        public IEnumerable<FactoryResult> GetDiscoverers(IReadOnlyCollection<string> sources, Settings.BoostTestAdapterSettings settings)
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
            var listContentSources = tmpSources
                .Where(s => Path.GetExtension(s) == BoostTestDiscoverer.ExeExtension)
                .Where(_listContentHelper.IsListContentSupported)
                .ToList();

            if (listContentSources.Count > 0)
            {
                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new ListContentDiscoverer(),
                    Sources = listContentSources
                });

                tmpSources.RemoveAll(s => listContentSources.Contains(s));
            }


            // sources that NOT support the list-content parameter
            var sourceCodeSources = tmpSources
                .Where(s => Path.GetExtension(s) == BoostTestDiscoverer.ExeExtension)
                .ToList();

            if (sourceCodeSources.Count > 0)
            {
                discoverers.Add(new FactoryResult()
                {
                    Discoverer = new SourceCodeDiscoverer(),
                    Sources = sourceCodeSources
                });
            }
            return discoverers;
        }

        #endregion
    }
}
