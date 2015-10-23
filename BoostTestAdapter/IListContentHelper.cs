using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter
{
    public interface IListContentHelper
    {
        bool IsListContentSupported(string exeName);
        string GetListContentOutput(string exeName);
        IDebugHelper CreateDebugHelper(string exeName);
    }
}
