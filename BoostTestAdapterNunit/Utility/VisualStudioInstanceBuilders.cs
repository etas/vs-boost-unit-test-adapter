// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using FakeItEasy;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// Builds fake IVisualStudio instances
    /// </summary>
    public class FakeVisualStudioInstanceBuilder
    {
        private ISolution _solution = null;

        /// <summary>
        /// Assigns a solution to the VisualStudio instance
        /// </summary>
        /// <param name="solution">The fake solution builder which is to be registered with this fake VisualStudio instance</param>
        /// <returns>this</returns>
        public FakeVisualStudioInstanceBuilder Solution(FakeSolutionBuilder solution)
        {
            return this.Solution(solution.Build());
        }

        /// <summary>
        /// Assigns a solution to the VisualStudio instance
        /// </summary>
        /// <param name="solution">The solution which is to be registered with this fake VisualStudio instance</param>
        /// <returns>this</returns>
        public FakeVisualStudioInstanceBuilder Solution(ISolution solution)
        {
            this._solution = solution;
            return this;
        }

        /// <summary>
        /// Commits any pending changes and builds a fake IVisualStudio instance.
        /// </summary>
        /// <returns>A fake IVisualStudio instance consisting of the previously registered solutions</returns>
        public IVisualStudio Build()
        {
            IVisualStudio fake = A.Fake<IVisualStudio>();

            A.CallTo(() => fake.Version).Returns("MockVisualStudioInstance");
            A.CallTo(() => fake.Solution).Returns(this._solution);

            return fake;
        }
    }

    /// <summary>
    /// Builds fake ISolution instances
    /// </summary>
    public class FakeSolutionBuilder
    {
        private string _name = string.Empty;
        private readonly IList<IProject> _projects = new List<IProject>();

        /// <summary>
        /// Identifies the name of the solution
        /// </summary>
        /// <param name="name">The name for the solution</param>
        /// <returns>this</returns>
        public FakeSolutionBuilder Name(string name)
        {
            this._name = name;
            return this;
        }

        /// <summary>
        /// Assigns a project to the solution
        /// </summary>
        /// <param name="project">The fake project builder which is to be registered with this fake solution instance</param>
        /// <returns>this</returns>
        public FakeSolutionBuilder Project(FakeProjectBuilder project)
        {
            return this.Project(project.Build());
        }

        /// <summary>
        /// Assigns a project to the solution
        /// </summary>
        /// <param name="project">The project which is to be registered with this fake solution instance</param>
        /// <returns>this</returns>
        public FakeSolutionBuilder Project(IProject project)
        {
            this._projects.Add(project);
            return this;
        }

        /// <summary>
        /// Commits any pending changes and builds a fake ISolution instance.
        /// </summary>
        /// <returns>A fake ISolution instance consisting of the previously registered name and projects</returns>
        public ISolution Build()
        {
            ISolution fake = A.Fake<ISolution>();

            A.CallTo(() => fake.Name).Returns(this._name);
            A.CallTo(() => fake.Projects).Returns(this._projects);

            return fake;
        }
    }

    /// <summary>
    /// Builds fake IProject instances
    /// </summary>
    public class FakeProjectBuilder
    {
        private string _name = string.Empty;
        private string _primaryOutput = string.Empty;
        private IList<string> _sourcesFullFilePath = new List<string>(); 
        private string _workingDirectory = string.Empty;
        private string _environment = string.Empty;

        /// <summary>
        /// Identifies the name of the project
        /// </summary>
        /// <param name="name">The name for the project</param>
        /// <returns>this</returns>
        public FakeProjectBuilder Name(string name)
        {
            this._name = name;
            return this;
        }

        /// <summary>
        /// Identifies the project's primary output location
        /// </summary>
        /// <param name="output">The primary output path</param>
        /// <returns>this</returns>
        public FakeProjectBuilder PrimaryOutput(string output)
        {
            this._primaryOutput = output;
            return this;
        }

        /// <summary>
        /// Identifies the project's working directory
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public FakeProjectBuilder WorkingDirectory(string output)
        {
            this._workingDirectory = output;
            return this;
        }

        /// <summary>
        /// Identifies the project's environment
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public FakeProjectBuilder Environment(string output)
        {
            this._environment = output;
            return this;
        }

        /// <summary>
        /// Commits any pending changes and builds a fake IProject instance.
        /// </summary>
        /// <returns>A fake IProject instance consisting of the previously registered output, definitions and sources</returns>
        public IProject Build()
        {
            IProject fake = A.Fake<IProject>();

            A.CallTo(() => fake.Name).Returns(this._name);

            IProjectConfiguration fakeConfiguration = A.Fake<IProjectConfiguration>();
            A.CallTo(() => fakeConfiguration.PrimaryOutput).Returns(this._primaryOutput);
           
            A.CallTo(() => fake.ActiveConfiguration).Returns(fakeConfiguration);

            IVSDebugConfiguration fakeVSConfiguration = A.Fake<IVSDebugConfiguration>();
            A.CallTo(() => fakeVSConfiguration.WorkingDirectory).Returns(this._workingDirectory);
            A.CallTo(() => fakeVSConfiguration.Environment).Returns(this._environment);

            A.CallTo(() => fakeConfiguration.VSDebugConfiguration).Returns(fakeVSConfiguration);

            return fake;
        }
    }

}
