using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoostTestAdapter;
using BoostTestAdapter.Utility;

namespace BoostTestAdapterNunit.Fakes
{
    class StubListContentHelper : IListContentHelper
    {
        public bool IsListContentSupported(string exeName)
        {
            if (exeName == "ListContentSupport.exe")
                return true;

            return false;
        }

        public string GetListContentOutput(string exeName)
        {
            if (exeName == "ListContentSupport.exe")
            {
                var str = "";
                str += "Test1*\n";
                str += "UnitTest1*\n";
                str += "    Test11*\n";
                str += "    Test12*\n";
                str += "UnitTest2*\n";
                str += "    NestedUnitTest21*\n";
                str += "        NestedUnitTest211*\n";
                str += "            Test2111*\n";
                str += "            DisabledTest2112\n";
                str += "            Test2113*\n";
                str += "Test2*\n";
                return str;
            }

            return string.Empty;
        }


        public IDebugHelper CreateDebugHelper(string exeName)
        {
            return new StubDebugHelper(exeName);
        }
    }
}
