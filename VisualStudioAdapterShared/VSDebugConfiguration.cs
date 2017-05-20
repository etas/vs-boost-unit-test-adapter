// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;

using Microsoft.VisualStudio.VCProjectEngine;

namespace VisualStudioAdapter.Shared
{
    /// <summary>
    /// Adapter class for a Visual Studio Project Configuration
    /// </summary>
    public class VSDebugConfiguration : IVSDebugConfiguration
    {
        private VCConfiguration _configuration = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The base Visual Studio project configuration which is to be adapted</param>
        public VSDebugConfiguration(VCConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this._configuration = configuration;
        }

        /// <summary>
        /// Returns read-only VCConfiguration object
        /// </summary>
        public VCConfiguration VCConfiguration
        {
            get
            {
                return this._configuration;
            }                        
        }

        #region IVSConfiguration

        /// <summary>
        /// Evaluates 'WorkingDirectory' from Visual Studio Configuration Properties
        /// </summary>
        public string WorkingDirectory
        {
            get
            {
                VCDebugSettings setting = this._configuration.DebugSettings as VCDebugSettings;
                return this._configuration.Evaluate(setting.WorkingDirectory);
            }
        }

        /// <summary>
        /// Evaluates 'Environment' from Visual Studio Configuration Properties
        /// </summary>
        public string Environment
        {
            get
            {
                VCDebugSettings setting = this._configuration.DebugSettings as VCDebugSettings;
                return this._configuration.Evaluate(setting.Environment);
            }
        }

        #endregion IVSConfiguration
    }
}
