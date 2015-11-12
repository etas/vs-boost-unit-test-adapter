using System;
using System.Collections.Generic;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Interface to an object able to read and search symbols of an executable.
    /// </summary>
    public interface IDebugHelper : IDisposable
    {
        /// <summary>
        /// Searches for a symbol with the name similar to the input.
        /// </summary>
        /// <param name="name">The name of the symbol to be searched.</param>
        /// <param name="symbols">All the symbols that contain the <paramref name="name"/> parameter in their name.</param>
        /// <returns>True if the search was performed without errors. False otherwise.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        bool LookupSymbol(string name, out IEnumerable<SymbolInfo> symbols);
    }
}