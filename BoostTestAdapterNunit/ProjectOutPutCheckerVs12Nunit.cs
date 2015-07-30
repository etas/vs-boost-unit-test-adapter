using CheckForProjectOutPut_VS12;
using CheckForprojectOutPut_VS13;
using FakeItEasy;
using NUnit.Framework;
using EnvDTE;
using Microsoft.VisualStudio.VCProjectEngine;

namespace BoostTestAdaptorNunit
{
    [TestFixture]
    class ProjectOutPutCheckerVs12Nunit
    {
        Project _fackProjObj;
        ConfigurationManager _fakeConfigurationManager;
        Configuration _fakeActiveConfiguration;
        VCProject _fackVcProject;
        IVCCollection _fakeCollection;
        VCConfiguration _fakeVcConfiguration;

        /**
         * Bellow test cases uses faked object for testing IsProjectOutputSame method
         * Faked objects are created using FakeItEasy and configured in testcases
         * If any method or property is not configured on fake object created it will try to call the actual implementation
         * call to actual implementation from fake object causes exception 
         */
        [TestFixtureSetUp]
        public void FakeTheProjectInterface()
        {
            _fackProjObj = A.Fake<Project>();
            _fakeConfigurationManager = A.Fake<ConfigurationManager>();
            _fakeActiveConfiguration = A.Fake<Configuration>();
            _fackVcProject = A.Fake<VCProject>();
            _fakeCollection = A.Fake<IVCCollection>();
            _fakeVcConfiguration = A.Fake<VCConfiguration>();
        }
        
        [Test]
        public void CheckOutputForVsProj2012_OutputPathMatchTrue()
        {
            A.CallTo(() => _fackProjObj.ConfigurationManager).Returns(_fakeConfigurationManager);
            A.CallTo(() => _fakeConfigurationManager.ActiveConfiguration).Returns(_fakeActiveConfiguration);
            A.CallTo(() => _fakeActiveConfiguration.ConfigurationName).Returns("Debug");
            A.CallTo(() => _fakeActiveConfiguration.PlatformName).Returns("Win32");
            A.CallTo(() => _fackProjObj.Object).Returns(_fackVcProject);
            A.CallTo(() => _fackVcProject.Configurations).Returns(_fakeCollection);
            A.CallTo(() => _fakeCollection.Item("Debug|Win32")).Returns(_fakeVcConfiguration);
            A.CallTo(() => _fakeVcConfiguration.PrimaryOutput).Returns("exePath");

            Assert.AreEqual(true, ProjectOutputCheckerVs12.IsProjectOutputSame(_fackProjObj, "exePath"));
        }

        [Test]
        public void CheckOutputForVsProj2012_OutputPathMatchFalse()
        {
            A.CallTo(() => _fackProjObj.ConfigurationManager).Returns(_fakeConfigurationManager);
            A.CallTo(() => _fakeConfigurationManager.ActiveConfiguration).Returns(_fakeActiveConfiguration);
            A.CallTo(() => _fakeActiveConfiguration.ConfigurationName).Returns("Release");
            A.CallTo(() => _fakeActiveConfiguration.PlatformName).Returns("Win32");
            A.CallTo(() => _fackProjObj.Object).Returns(_fackVcProject);
            A.CallTo(() => _fackVcProject.Configurations).Returns(_fakeCollection);
            A.CallTo(() => _fakeCollection.Item("Release|Win32")).Returns(_fakeVcConfiguration);
            A.CallTo(() => _fakeVcConfiguration.PrimaryOutput).Returns("exePathDiff");

            Assert.AreEqual(false, ProjectOutputCheckerVs12.IsProjectOutputSame(_fackProjObj, "exePath"));
        }

        [Test]
        public void CheckOutputForVsProj2013_OutputPathMatchFalse()
        {

            A.CallTo(() => _fackProjObj.ConfigurationManager).Returns(_fakeConfigurationManager);
            A.CallTo(() => _fakeConfigurationManager.ActiveConfiguration).Returns(_fakeActiveConfiguration);
            A.CallTo(() => _fakeActiveConfiguration.ConfigurationName).Returns("Release");
            A.CallTo(() => _fakeActiveConfiguration.PlatformName).Returns("Win32");
            A.CallTo(() => _fackProjObj.Object).Returns(_fackVcProject);
            A.CallTo(() => _fackVcProject.Configurations).Returns(_fakeCollection);
            A.CallTo(() => _fakeCollection.Item("Release|Win32")).Returns(_fakeVcConfiguration);
            A.CallTo(() => _fakeVcConfiguration.PrimaryOutput).Returns("exePathDiff");

            Assert.AreEqual(false, ProjectOutputCheckerVs13.IsProjectOutputSame(_fackProjObj, "exePath"));
        }

        [TestFixtureTearDown]
        public void ClearTheFakeObject()
        {
            _fackProjObj = null;
            _fakeActiveConfiguration = null;
            _fakeConfigurationManager = null;
            _fakeCollection = null;
            _fakeVcConfiguration = null;
        }
    }
}
