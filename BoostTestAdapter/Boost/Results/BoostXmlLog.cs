// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Globalization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using BoostTestAdapter.Boost.Results.LogEntryTypes;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results
{
    /// <summary>
    /// Boost Xml Log
    /// </summary>
    public class BoostXmlLog : BoostTestResultXMLOutput
    {
        #region Constants

        /// <summary>
        /// Xml constants
        /// </summary>
        private static class Xml
        {
            public const string TestLog = "TestLog";
            public const string TestSuite = "TestSuite";
            public const string TestCase = "TestCase";
            public const string Name = "name";
            public const string TestingTime = "TestingTime";
            public const string Info = "Info";
            public const string Message = "Message";
            public const string Warning = "Warning";
            public const string Error = "Error";
            public const string FatalError = "FatalError";
            public const string Exception = "Exception";
            public const string LastCheckpoint = "LastCheckpoint";
            public const string Context = "Context";
            public const string Frame = "Frame";
            public const string File = "file";
            public const string Line = "line";
        }

        #endregion Constants

        #region Constructors
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The destination result collection. Possibly used for result aggregation.</param>
        public BoostXmlLog(IDictionary<string, TestResult> target)
            : base(target)
        {
        }

        #endregion Constructors

        #region BoostTestResultXMLOutput

        protected override IDictionary<string, TestResult> ParseXml(string xml)
        {
            // serge: now log output for dependent test cases supported, the have additional XML
            // element, that corrupts XML document structure
            using (XmlTextReader xtr = new XmlTextReader(xml, XmlNodeType.Element, null))
            {
                while (xtr.Read())
                {
                    if (xtr.NodeType == XmlNodeType.Element && xtr.Name == Xml.TestLog)
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlElement elemTestLog = doc.CreateElement(Xml.TestLog);
                        elemTestLog.InnerXml = xtr.ReadInnerXml();
                        ParseTestUnitsLog(elemTestLog.ChildNodes, new QualifiedNameBuilder(), Target);
                        break;
                    }
                }
            }

            return Target;
        }

        #endregion BoostTestResultXMLOutput

        /// <summary>
        /// Parses child TestUnit nodes.
        /// </summary>
        /// <param name="nodes">The collection of Xml nodes which are valid TestUnit nodes.</param>
        /// <param name="path">The QualifiedNameBuilder which hosts the current fully qualified path.</param>
        /// <param name="collection">The test result collection which will host the result.</param>
        private static void ParseTestUnitsLog(XmlNodeList nodes, QualifiedNameBuilder path, IDictionary<string, TestResult> collection)
        {
            foreach (XmlNode child in nodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.Name == Xml.TestSuite)
                    {
                        ParseTestSuiteLog(child, path, collection);
                    }
                    else if (child.Name == Xml.TestCase)
                    {
                        ParseTestCaseLog(child, path, collection);
                    }
                }
            }
        }

        /// <summary>
        /// Parses a TestSuite log node.
        /// </summary>
        /// <param name="node">The TestSuite Xml node to parse.</param>
        /// <param name="path">The QualifiedNameBuilder which hosts the current fully qualified path.</param>
        /// <param name="collection">The test result collection which will host the result.</param>
        private static void ParseTestSuiteLog(XmlNode node, QualifiedNameBuilder path, IDictionary<string, TestResult> collection)
        {
            path.Push(node.Attributes[Xml.Name].Value);

            ParseTestUnitsLog(node.ChildNodes, path, collection);

            path.Pop();
        }

        /// <summary>
        /// Parses a TestCase log node.
        /// </summary>
        /// <param name="node">The TestCase Xml node to parse.</param>
        /// <param name="path">The QualifiedNameBuilder which hosts the current fully qualified path.</param>
        /// <param name="collection">The test result collection which will host the result.</param>
        private static void ParseTestCaseLog(XmlNode node, QualifiedNameBuilder path, IDictionary<string, TestResult> collection)
        {
            // Temporarily push TestCase on TestSuite name builder to acquire the fully qualified name of the TestCase
            path.Push(node.Attributes[Xml.Name].Value);

            var currentPath = path.ToString();

            // Acquire result record of this TestCase
            TestResult result = null;
            if (!collection.TryGetValue(currentPath, out result))
            {
                result = new TestResult(collection);
                collection[currentPath] = result;
            }

            // Reset path to original value
            path.Pop();

            XmlNode testingTime = node.SelectSingleNode(Xml.TestingTime);

            if (testingTime != null)
            {
                // Boost test testing time is listed in microseconds
                result.Duration += ulong.Parse(testingTime.InnerText, CultureInfo.InvariantCulture);
            }

            ParseTestCaseLogEntries(node.ChildNodes, result);
        }

        /// <summary>
        /// Parses Log Entries from the collection of log nodes.
        /// </summary>
        /// <param name="nodes">The collection of Xml nodes which are valid LogEntry nodes.</param>
        /// <param name="result">The TestResult which will host the parsed LogEntries.</param>
        private static void ParseTestCaseLogEntries(XmlNodeList nodes, TestResult result)
        {
            foreach (XmlNode child in nodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    LogEntry entry = null;

                    switch (child.Name)
                    {
                        case Xml.Info: entry        = ParseLogEntry<LogEntryInfo>(child); break;
                        case Xml.Message: entry     = ParseLogEntry<LogEntryMessage>(child); break;
                        case Xml.Warning: entry     = ParseLogEntry<LogEntryWarning>(child); break;
                        case Xml.Error: entry       = ParseLogEntry<LogEntryError>(child); break;
                        case Xml.FatalError: entry  = ParseLogEntry<LogEntryFatalError>(child); break;
                        case Xml.Exception: entry   = ParseLogException(child); break;
                        default: entry              = null; break;
                    }

                    if (entry != null)
                    {
                        result.LogEntries.Add(entry);
                    }
                }
            }
        }

        /// <summary>
        /// Parse SourceFileInfo from the provided node.
        /// </summary>
        /// <param name="node">The Xml node which contains source file information.</param>
        /// <returns>A SourceFileInfo populated from the provided Xml node.</returns>
        private static SourceFileInfo ParseSourceInfo(XmlNode node)
        {
            SourceFileInfo info = null;

            XmlAttribute file = node.Attributes[Xml.File];
            if (file != null)
            {
                info = new SourceFileInfo(file.Value);
            }

            if (info != null)
            {
                XmlAttribute line = node.Attributes[Xml.Line];
                if (line != null)
                {
                    info.LineNumber = int.Parse(line.Value, CultureInfo.InvariantCulture);
                }
            }

            return info;
        }

        /// <summary>
        /// Parse a LogEntryException from the provided node.
        /// </summary>
        /// <param name="node">The Xml node which contains exception information.</param>
        /// <returns>A LogEntryException populated from the provided Xml node.</returns>
        private static LogEntryException ParseLogException(XmlNode node)
        {
            LogEntryException exception = ParseLogEntry<LogEntryException>(node);

            foreach (XmlNode child in node.ChildNodes)
            {
                if ((child.NodeType == XmlNodeType.Element) && (child.Name == Xml.LastCheckpoint))
                {
                    exception.LastCheckpoint = ParseSourceInfo(child);
                    exception.CheckpointDetail = child.InnerText;
                }
            }

            return exception;
        }
        
        /// <summary>
        /// Populates a LogEntry from the provided node with the default information.
        /// </summary>
        /// <param name="node">The Xml node which contains the log information.</param>
        /// <param name="entry">The LogEntry to be populated.</param>
        /// <returns>entry</returns>
        private static T ParseLogEntry<T>(XmlNode node) where T : LogEntry, new()
        {
            T entry = new T();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.CDATA)
                {
                    entry.Detail = child.InnerText;
                }
                else if ((child.NodeType == XmlNodeType.Element) && (child.Name == Xml.Context))
                {
                    entry.ContextFrames = child.SelectNodes(Xml.Frame).Cast<XmlNode>().Select(frame => frame.InnerText.Trim()).ToList();
                }
            }

            entry.Source = ParseSourceInfo(node);

            return entry;
        }
    }
}