// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace VisualStudio2015Adapter
{
    /// <summary>
    /// Adapter for a Visual Studio 2015 running instance.
    /// </summary>
    public class VisualStudio : VisualStudioAdapter.Shared.VisualStudio
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dte")]
        public VisualStudio(EnvDTE80.DTE2 dte) :
            base(dte, "14")
        {
        }
    }
}