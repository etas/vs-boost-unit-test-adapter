using System;
using System.Collections.Generic;
using System.Linq;
using VSProject = EnvDTE.Project;
using VSProjectItem = EnvDTE.ProjectItem;
using VSSolution = EnvDTE.Solution;

namespace VisualStudioAdapter.Shared
{
    /// <summary>
    /// Adapter for a Visual Studio solution
    /// </summary>
    public class Solution : ISolution
    {
        /// <summary>
        /// Constant strings which distinguish Solution item kinds.
        /// </summary>
        private static class EnvDTEProjectKinds
        {
            /// <summary>
            /// Solution folder item kind label
            /// </summary>
            public const string VsProjectKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

            /// <summary>
            /// C++ project item kind label
            /// </summary>
            public const string VsProjectKindVCpp = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";
        }

        private VSSolution _solution = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="solution">The base Visual Studio solution reference</param>
        public Solution(VSSolution solution)
        {
            if (solution == null) throw new ArgumentNullException("solution");

            this._solution = solution;
            this.Name = solution.FullName;
        }

        #region ISolution

        public string Name { get; private set; }

        public IEnumerable<IProject> Projects
        {
            get
            {
                foreach (VSProject folderOrProject in this._solution.Projects.OfType<VSProject>())
                {
                    //Call 364853
                    //Loop through the solution folders (if any) to get all the projects within a solution
                    foreach (IProject project in GetProjects(folderOrProject))
                    {
                        yield return project;
                    }
                }
            }
        }

        #endregion ISolution

        /// <summary>
        /// Recursively retrieves projects form the provided Visual Studio project
        /// </summary>
        /// <param name="folderOrProject">A reference to a Visual Studio Project or Solution Folder</param>
        /// <returns>An enumeration of all Visual Studio projects (only)</returns>
        private IEnumerable<IProject> GetProjects(VSProject folderOrProject)
        {
            // it is a solution folder
            if (folderOrProject.Kind == EnvDTEProjectKinds.VsProjectKindSolutionFolder)
            {
                foreach (VSProjectItem item in folderOrProject.ProjectItems)
                {
                    // it is a project
                    if (item.SubProject != null)
                    {
                        foreach (IProject project in GetProjects(item.SubProject))
                        {
                            yield return project;
                        }
                    }
                }
            }
            else if (folderOrProject.Kind == EnvDTEProjectKinds.VsProjectKindVCpp)
            {
                yield return new Project(folderOrProject);
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