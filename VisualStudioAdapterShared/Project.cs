// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using Microsoft.VisualStudio.VCProjectEngine;

namespace VisualStudioAdapter.Shared
{
    /// <summary>
    /// Adapter for a Visual Studio Project
    /// </summary>
    public class Project : IProject
    {
        private EnvDTE.Project _project = null;
        private ProjectConfiguration _configuration = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The EnvDTE.Project which is to be adapted</param>
        public Project(EnvDTE.Project project)
        {
            if (project == null) throw new ArgumentNullException("project");

            this._project = project;
            this.Name = project.FullName;
        }

        #region IProject

        public string Name { get; private set; }

        public IProjectConfiguration ActiveConfiguration
        {
            get
            {
                if (this._configuration == null)
                {
                    VCConfiguration configuration = this.Configuration;

                    // Cache the adapted configuration in case it is requested multiple times
                    this._configuration = (configuration == null) ? null : new ProjectConfiguration(new VSDebugConfiguration(configuration));
                }

                return this._configuration;
            }
        }

        #endregion IProject

        /// <summary>
        /// Retrieves the active configuration from the base Visual Studio Project
        /// </summary>
        private VCConfiguration Configuration
        {
            get
            {
                // Cast to a specific VS201* VCProject instance
                VCProject vcProj = this._project.Object as VCProject;
                if (vcProj != null)
                {
                    var configs = vcProj.Configurations as IVCCollection;
                    if (configs != null)
                    {
                        return configs.Item(ActiveConfigurationName) as VCConfiguration;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Provides the currently active project configuration name
        /// </summary>
        private string ActiveConfigurationName
        {
            get
            {
                return this._project.ConfigurationManager.ActiveConfiguration.ConfigurationName + "|" + this._project.ConfigurationManager.ActiveConfiguration.PlatformName;
            }
        }

        #region Object Overrides

        public override string ToString()
        {
            return this.Name;
        }

        #endregion Object Overrides
    }
}