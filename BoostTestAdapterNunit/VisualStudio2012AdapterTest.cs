using EnvDTE;
using FakeItEasy;
using Microsoft.VisualStudio.VCProjectEngine;
using NUnit.Framework;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class VisualStudio2012AdapterTest
    {
        #region Test Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.FakeVS = new FakeVisualStudio2012();
            this.Project = new VisualStudioAdapter.Shared.Project(this.FakeVS.Project);
        }

        #endregion Test Setup/Teardown

        #region Test Data

        private FakeVisualStudio2012 FakeVS { get; set; }
        private IProject Project { get; set; }
        private const string DefaultProjectName = "SampleProject";
        private const string DefaultOutput = "test.boostd.exe";

        #endregion Test Data

        #region Helper Classes

        /// <summary>
        /// Aggregates fake Visual Studio EnvDTE and VCProjectEngine structures
        /// </summary>
        private class FakeVisualStudio2012
        {
            public FakeVisualStudio2012()
            {
                this.Project = A.Fake<Project>();
                this.ConfigurationManager = A.Fake<ConfigurationManager>();
                this.ActiveConfiguration = A.Fake<Configuration>();
                this.VCProject = A.Fake<VCProject>();
                this.ConfigurationCollection = A.Fake<IVCCollection>();
                this.VCConfiguration = A.Fake<VCConfiguration>();

                A.CallTo(() => this.Project.FullName).Returns(DefaultProjectName);
                A.CallTo(() => this.Project.ConfigurationManager).Returns(this.ConfigurationManager);
                
                A.CallTo(() => this.ConfigurationManager.ActiveConfiguration).Returns(this.ActiveConfiguration);
                A.CallTo(() => this.Project.Object).Returns(this.VCProject);
                
                A.CallTo(() => this.ActiveConfiguration.ConfigurationName).Returns("Debug");
                A.CallTo(() => this.ActiveConfiguration.PlatformName).Returns("Win32");
                
                A.CallTo(() => this.VCProject.Configurations).Returns(this.ConfigurationCollection);
                
                A.CallTo(() => this.ConfigurationCollection.Item("Debug|Win32")).Returns(this.VCConfiguration);
                
                A.CallTo(() => this.VCConfiguration.PrimaryOutput).Returns(DefaultOutput);
            }

            public Project Project { get; private set; }
            private ConfigurationManager ConfigurationManager { get; set; }
            private Configuration ActiveConfiguration { get; set; }
            private VCProject VCProject { get; set; }
            private IVCCollection ConfigurationCollection { get; set; }
            private VCConfiguration VCConfiguration { get; set; }
        }

        #endregion Helper Classes

        #region Tests

        /// <summary>
        /// Visual Studio 2012 adaptation.
        /// 
        /// Test aims:
        ///     - Ensure that EnvDTE and VCProjectEngine structures are properly adapted to VisualStudioAdapter structures.
        /// </summary>
        [Test]
        public void VisualStudio2012Adaptation()
        {
            Assert.That(this.Project.Name, Is.EqualTo(DefaultProjectName));
            Assert.That(this.Project.ActiveConfiguration.PrimaryOutput, Is.EqualTo(DefaultOutput));
        }

        #endregion Tests
    }
}
