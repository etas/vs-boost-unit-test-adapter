using System;
using System.Collections.Generic;

namespace BoostTestAdapter.Utility
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dbg")]
    public interface IDebugHelper : IDisposable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        bool LookupSymbol(string name, out IEnumerable<SymbolInfo> symbols);
    }
}