// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    internal class CorrectReferencesAssembliesTest
    {
        /// <summary>
        /// It is critical that assemblies are compiled with the correct references.  The scope of these tests are to ensure that a user
        /// is referencing the correct Microsoft.VisualStudio.TestPlatform.ObjectModel and the correct Microsoft.VisualStudio.VCProjectEngine
        /// in all the complied assemblies. The idea is taken off the Nunit tests of NUnit nunit3-vs-adapter/src/NUnitTestAdapterTests/ProjectTests.cs on GitHub
        /// </summary>
        /// <param name="dll">the dll name</param>
        /// <param name="assemblyReferenceName">the assembly that we are going to check that is properly referenced</param>
        /// <param name="versionMajor">version number that assembly must have</param>
        [TestCase("BoostTestAdapter.dll", "Microsoft.VisualStudio.TestPlatform.ObjectModel", 11, TestName = "CorrectlyReferencedBoostTestAdapter", Description = "Microsoft.VisualStudio.TestPlatform.ObjectModel in BoostTestAdapter must point to the VS2012 version")]
        [TestCase("VisualStudio2012Adapter.dll", "Microsoft.VisualStudio.VCProjectEngine", 11, TestName = "CorrectlyReferencedVisualStudio2012Adapter", Description = "Microsoft.VisualStudio.VCProjectEngine in VisualStudio2012Adapter must point to the VS2012 version")]
        [TestCase("VisualStudio2013Adapter.dll", "Microsoft.VisualStudio.VCProjectEngine", 12, TestName = "CorrectlyReferencedVisualStudio2013Adapter", Description = "Microsoft.VisualStudio.VCProjectEngine in VisualStudio2013Adapter must point to the VS2013 version")]
        [TestCase("VisualStudio2015Adapter.dll", "Microsoft.VisualStudio.VCProjectEngine", 14, TestName = "CorrectlyReferencedVisualStudio2015Adapter", Description = "Microsoft.VisualStudio.VCProjectEngine in VisualStudio2015Adapter must point to the VS2015 version")]
        public void CorrectReferences(string dll, string assemblyReferenceName, int versionMajor)
        {
            var assembly = Assembly.LoadFrom(TestContext.CurrentContext.TestDirectory + "/" + dll);
            var referencedAssemblies = assembly.GetReferencedAssemblies().Where(ass => ass.Name == assemblyReferenceName).ToList();
            Assert.IsTrue(referencedAssemblies != null && referencedAssemblies.Count() == 1, "No reference to " + assemblyReferenceName + " found");
            Assert.IsTrue(referencedAssemblies[0].Version.Major == versionMajor,
                assemblyReferenceName + " in " + dll + " is referenced to an incorrect version");
        }
    }
}