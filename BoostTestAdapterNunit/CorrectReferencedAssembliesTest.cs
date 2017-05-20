// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using System.IO;

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
        [TestCase("BoostTestAdapter.TestAdapter.dll", "Microsoft.VisualStudio.TestPlatform.ObjectModel", 14, TestName = "CorrectlyReferencedBoostTestAdapter", Description = "Microsoft.VisualStudio.TestPlatform.ObjectModel in BoostTestAdapter must point to the VS2015 version")]
        [TestCase("VisualStudio2015Adapter.dll", "Microsoft.VisualStudio.VCProjectEngine", 14, TestName = "CorrectlyReferencedVisualStudio2015Adapter", Description = "Microsoft.VisualStudio.VCProjectEngine in VisualStudio2015Adapter must point to the VS2015 version")]
        public void CorrectReferences(string dll, string assemblyReferenceName, int versionMajor)
        {
            var assembly = Assembly.LoadFrom(Path.Combine(TestContext.CurrentContext.TestDirectory, dll));
            var referencedAssembly = assembly.GetReferencedAssemblies().FirstOrDefault(reference => (reference.Name == assemblyReferenceName));

            Assert.That(referencedAssembly, Is.Not.Null, ("No reference to " + assemblyReferenceName + " found"));
            Assert.That(referencedAssembly.Version.Major, Is.EqualTo(versionMajor), (assemblyReferenceName + " in " + dll + " is referenced to an incorrect version"));
        }
    }
}