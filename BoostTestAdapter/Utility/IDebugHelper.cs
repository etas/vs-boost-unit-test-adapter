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
        /// Searches for a symbol with the the <b>exact</b> provided name.
        /// </summary>
        /// <param name="name">The name of the symbol to be searched.</param>
        /// <returns>The found symbol with the given name or null if one cannot be found.</returns>
        SymbolInfo LookupSymbol(string name);

        /// <summary>
        /// Determines whether or not a symbol with the <b>exact</b> provided name is available.
        /// </summary>
        /// <param name="name">The name of the symbol to search for.</param>
        /// <returns>true if the symbol is available; false otherwise.</returns>
        bool ContainsSymbol(string name);
    }
}