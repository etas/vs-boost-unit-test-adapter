using System;
using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter.Utility.VisualStudio;
using VisualStudioAdapter;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// Builds and emulates a Visual Studio solution with a single test project.
    /// Copies resource files to their intended location for later reference.
    /// </summary>
    public class DummySolution : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The fully qualified path of a test source</param>
        /// <param name="sourceFileName">The embedded resource file name which is to be considered as the sole source file for this test project</param>
        public DummySolution(string source, string sourceFileName) :
            this(source, new string[] { sourceFileName })
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The fully qualified path of a test source</param>
        /// <param name="sourceFileNames">An enumeration of embedded resource file names which are to be considered as source files for this test project</param>
        public DummySolution(string source, IEnumerable<string> sourceFileNames)
        {
            this.Source = source;
            this.SourceFiles = sourceFileNames;

            this.SourceFileResourcePaths = new List<DummySourceFile>();

            SetUpSolutionEnvironment();
            SetUpSolution();
        }

        /// <summary>
        /// Sets up the solution environment by copying the embedded resources to disk
        /// for later reference by the necessary algorithms.
        /// </summary>
        private void SetUpSolutionEnvironment()
        {
            foreach (string source in this.SourceFiles)
            {
                this.SourceFileResourcePaths.Add( new DummySourceFile(source) );
            }
        }

        /// <summary>
        /// Sets up a fake Visual Studio environment consisting of a solution with 1 test project whose
        /// primary output is the specified construction-time source and the respective code file sources.
        /// </summary>
        private void SetUpSolution()
        {
            IList<string> sources = this.SourceFileResourcePaths.Select(source => source.TempSourcePath).ToList();

            IVisualStudio vs = new FakeVisualStudioInstanceBuilder().
                Solution(
                    new FakeSolutionBuilder().
                        Name("SampleSolution").
                        Project(
                            new FakeProjectBuilder().
                                Name("SampleProject").
                                PrimaryOutput(this.Source).
                                Sources(sources)
                        )
                ).
                Build();

            this.Provider = new DummyVSProvider(vs);
        }

        /// <summary>
        /// The test source
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// The test code source files
        /// </summary>
        public IEnumerable<string> SourceFiles { get; private set; }

        /// <summary>
        /// The respective DummySourceFiles for the test code source files
        /// </summary>
        public ICollection<DummySourceFile> SourceFileResourcePaths { get; private set; }

        /// <summary>
        /// The IVisualStudio instance provider for this DummySolution
        /// </summary>
        public IVisualStudioInstanceProvider Provider { get; private set; }

        #region System.IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (DummySourceFile resource in this.SourceFileResourcePaths)
                {
                    resource.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion System.IDisposable
    }
}
