// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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
        private VSDebugConfiguration _configuration = null;
        private VCppCompilerOptions _cppCompiler = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The base Visual Studio project configuration which is to be adapted</param>
        public ProjectConfiguration(VSDebugConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            
            this._configuration = configuration;

            this.PrimaryOutput = this._configuration.VCConfiguration.PrimaryOutput;            
        }

        #region IProjectConfiguration

        public string PrimaryOutput { get; private set; }
        
        public IVCppCompilerOptions CppCompilerOptions
        {
            get
            {
                if (this._cppCompiler == null)
                {                    
                    IVCCollection tools = this._configuration.VCConfiguration.Tools as IVCCollection;
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

        public IVSDebugConfiguration VSDebugConfiguration
        {
            get
            {
                return _configuration;
            }            
        }

        #endregion IProjectConfiguration
    }
}