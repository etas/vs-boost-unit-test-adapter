// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results.LogEntryTypes
{
    /// <summary>
    /// Log entry for an identified memory leak.
    /// </summary>
    public class LogEntryMemoryLeak : LogEntry
    {
        private const string MemoryLeakNotification = "Memory leaks have been been detected. Please refer to the output tab for more details.";

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LogEntryMemoryLeak()
        {
            this.Detail = MemoryLeakNotification;
        }

        /// <summary>
        /// Constructor accepting a SourceFileInfo object
        /// </summary>
        /// <param name="source">Source file information related to this log message. May be null.</param>
        public LogEntryMemoryLeak(SourceFileInfo source)
            : base(source)
        {
        }

        public LogEntryMemoryLeak(string leakSourceFilePath, string leakSourceFileName, uint? leakLineNumber, uint? leakSizeInBytes, uint? leakMemoryAllocationNumber, string leakLeakedDataContents)
        {
            this.LeakSourceFilePath = leakSourceFilePath;
            this.LeakSourceFileName = leakSourceFileName;
            this.LeakLineNumber = leakLineNumber;
            this.LeakSizeInBytes = leakSizeInBytes;
            this.LeakMemoryAllocationNumber = leakMemoryAllocationNumber;
            this.LeakLeakedDataContents = leakLeakedDataContents;
            this.Detail = MemoryLeakNotification;
        }

        #endregion Constructors

        /// <summary>
        /// returns a string with the description of the class
        /// </summary>
        /// <returns>string having the description of the class</returns>
        public override string ToString()
        {
            return "Memory leak";
        }

        /// <summary>
        /// Indicates the source file path where the leak was detected at
        /// </summary>
        /// <remarks>If null, the information regarding the leak source file path is not available.  This generally is because the macro to replace the C++ operator new has not been utilized in the test project.</remarks>
        public string LeakSourceFilePath { get; set; }

        /// <summary>
        /// Indicates the source filename where the leak was detected at
        /// </summary>
        /// <remarks>If null, the information regarding the leak source file name is not available.  This generally is because the macro to replace the C++ operator new has not been utilized in the test project.</remarks>
        public string LeakSourceFileName { get; set; }

        /// <summary>
        /// Line number (respective the source file specified in property LeakSourceFileName) where the leak is detected at
        /// </summary>
        /// <remarks>If null, the information regarding the leak line number is not available.  This generally is because the macro to replace the C++ operator new has not been utilized in the test project, or the parsing of the line number failed.</remarks>
        public uint? LeakLineNumber { get; set; }

        /// <summary>
        /// Number of bytes leaked.
        /// </summary>
        /// <remarks>The Boost UTF always reports the leak size in bytes. If null, then it means that the parsing of the respective console output failed.</remarks>
        public uint? LeakSizeInBytes { get; set; }

        /// <summary>
        /// Property containing the memory allocation number of the leak/
        /// </summary>
        /// <remarks>The Boost UTF always reports the Memory allocation number whenever a memory leak is reported. If null, then it means that the parsing of the respective console output failed.</remarks>
        public uint? LeakMemoryAllocationNumber { get; set; }

        /// <summary>
        /// Property containing the leaked data contents as reported by Boost UTF
        /// </summary>
        public string LeakLeakedDataContents { get; set; }

        /// <summary>
        /// Property serves as an indicator wheather the source file and the line number information for the memory leak were available or not
        /// </summary>
        public bool LeakSourceFileAndLineNumberReportingActive { get; set; }
    }
}