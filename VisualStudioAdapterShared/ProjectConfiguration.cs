using System;
using System.Linq;

using Microsoft.VisualStudio.VCProjectEngine;

namespace VisualStudioAdapter.Shared
{
    /// <summary>
    /// Adapter class for a Visual Studio Project Configuration
    /// </summary>
    public class ProjectConfiguration : IProjectConfiguration
    {
        private VCConfiguration _configuration = null;
        private VCppCompilerOptions _cppCompiler = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The base Visual Studio project configuration which is to be adapted</param>
        public ProjectConfiguration(VCConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this._configuration = configuration;
            this.PrimaryOutput = configuration.PrimaryOutput;
        }

        #region IProjectConfiguration

        public string PrimaryOutput { get; private set; }

        public IVCppCompilerOptions CppCompilerOptions
        {
            get
            {
                if (this._cppCompiler == null)
                {
                    IVCCollection tools = this._configuration.Tools as IVCCollection;
                    if (tools != null)
                    {
                        VCCLCompilerTool compiler = tools.OfType<VCCLCompilerTool>().FirstOrDefault();
                        if (compiler != null)
                        {
                            this._cppCompiler = new VCppCompilerOptions(this._configuration, compiler);
                        }
                    }
                }

                return this._cppCompiler;
            }
        }

        #endregion IProjectConfiguration
    }
}