// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
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
            Detail = MemoryLeakNotification;
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
        /// Constructs a LogEntryMemoryLeak and populates the main components
        /// </summary>
        /// <param name="leakLocation">The location of the memory leak.</param>
        /// <param name="leakSizeInBytes">The number of bytes leaked.</param>
        /// <param name="leakMemoryAllocationNumber">The memory allocation number.</param>
        /// <param name="leakLeakedDataContents">The memory contents which were leaked.</param>
        /// <returns>A new LogEntryMemoryLeak instance populated accordingly</returns>
        public static LogEntryMemoryLeak MakeLogEntryMemoryLeak(SourceFileInfo leakLocation, uint? leakSizeInBytes, uint? leakMemoryAllocationNumber, string leakLeakedDataContents)
        {
            return new LogEntryMemoryLeak()
            {
                Source = leakLocation,
                LeakSizeInBytes = leakSizeInBytes,
                LeakMemoryAllocationNumber = leakMemoryAllocationNumber,
                LeakLeakedDataContents = leakLeakedDataContents
            };
        }
    }
}