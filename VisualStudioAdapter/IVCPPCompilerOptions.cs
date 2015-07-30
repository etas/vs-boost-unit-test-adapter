namespace VisualStudioAdapter
{
    /// <summary>
    /// Abstraction for Visual Studio C++ Compiler Options
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
    public interface IVCppCompilerOptions
    {
        /// <summary>
        /// Lists all Preprocessor defines currently configured. This includes all properties, even those configured within property sheets.
        /// </summary>
        Defines PreprocessorDefinitions { get; }
    }
}