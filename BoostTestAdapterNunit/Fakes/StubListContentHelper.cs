// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter;
using BoostTestAdapter.Utility;
using BoostTestAdapterNunit.Utility;

namespace BoostTestAdapterNunit.Fakes
{
    /// <summary>
    /// A fake implementation of a IListContentHelper. 
    /// It simulates the output of a typical Boost test executable.
    /// </summary>
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
                var output = TestHelper.ReadEmbeddedResource(
                        "BoostTestAdapterNunit.Resources.ListContentSupport.SampleListContentOutput.txt");

                return output;
            }

            return string.Empty;
        }


        public IDebugHelper CreateDebugHelper(string exeName)
        {
            return new StubDebugHelper(exeName);
        }
    }
}
