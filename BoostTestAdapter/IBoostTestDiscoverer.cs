using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace BoostTestAdapter
{
    public interface IBoostTestDiscoverer
    {
        void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink);
    }
}