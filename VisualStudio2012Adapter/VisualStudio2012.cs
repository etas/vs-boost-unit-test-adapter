namespace VisualStudio2012Adapter
{
    /// <summary>
    /// Adapter for a Visual Studio 2012 running instance.
    /// </summary>
    public class VisualStudio : VisualStudioAdapter.Shared.VisualStudio
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dte")]
        public VisualStudio(EnvDTE80.DTE2 dte) :
            base(dte, "11")
        {
        }
    }
}