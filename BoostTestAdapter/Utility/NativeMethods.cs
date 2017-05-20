// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace BoostTestAdapter.Utility
{
    public static class NativeMethods
    {
        private const int S_OK = 0;

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        /// <summary>
        ///     Get a snapshot of the running object table (ROT).
        /// </summary>
        /// <returns>
        ///     A collection of pairs mapping the name of the object
        ///     in the ROT to the corresponding object
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CreateBindCtx")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetObject"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetRunningObjectTable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IRunningObjectTable")]
        public static IEnumerable<KeyValuePair<string, object> > RunningObjectTable
        {
            get
            {
                var numFetched = new IntPtr();
                IRunningObjectTable runningObjectTable;
                IEnumMoniker monikerEnumerator;
                var monikers = new IMoniker[1];

                var runningObjectTableReturnCode = GetRunningObjectTable(0, out runningObjectTable); //This function can return the standard return values E_UNEXPECTED and S_OK.

                if (runningObjectTableReturnCode != S_OK)
                {
                    throw new ROTException("GetRunningObjectTable returned with code:" + runningObjectTableReturnCode);
                }

                runningObjectTable.EnumRunning(out monikerEnumerator);
                monikerEnumerator.Reset();

                while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
                {
                    IBindCtx ctx;

                    var createBindCtxReturnCode = CreateBindCtx(0, out ctx); //This function can return the standard return values E_OUTOFMEMORY and S_OK

                    if (createBindCtxReturnCode != S_OK)
                    {
                        // Avoid throwing at this point. Skip entry and continue the iteration.
                        continue;
                    }

                    string runningObjectName;
                    monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                    object runningObjectVal;
                    //usage is described at https://msdn.microsoft.com/en-us/library/windows/desktop/ms683841(v=vs.85).aspx
                    var getObjectReturnValue = runningObjectTable.GetObject(monikers[0], out runningObjectVal);  //This function call can return the standard return values S_FALSE and S_OK

                    if (getObjectReturnValue != S_OK)
                    {
                        runningObjectVal = null;
                    }

                    yield return new KeyValuePair<string, object>(runningObjectName, runningObjectVal);
                }
            }
        }
    }
}