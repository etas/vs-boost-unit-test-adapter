using System.Globalization;
using System.IO;
using System.Xml;
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
            public const string File = "file";
            public const string Line = "line";
        }

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Constructor accepting a path to the external file
        /// </summary>
        /// <param name="path">The path to an external file. File will be opened on construction.</param>
        public BoostXmlLog(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Constructor accepting a stream to the file contents
        /// </summary>
        /// <param name="stream">The file content stream.</param>
        public BoostXmlLog(Stream stream)
            : base(stream)
        {
        }

        #endregion Constructors

        #region IBoostOutputParser

        public override void Parse(TestResultCollection collection)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(this.InputStream);

            if (doc.DocumentElement.Name == Xml.TestLog)
            {
                ParseTestUnitsLog(doc.DocumentElement.ChildNodes, new QualifiedNameBuilder(), collection);
            }
        }

        #endregion IBoostOutputParser

        /// <summary>
        /// Parses child TestUnit nodes.
        /// </summary>
        /// <param name="nodes">The collection of Xml nodes which are valid TestUnit nodes.</param>
        /// <param name="path">The QualifiedNameBuilder which hosts the current fully qualified path.</param>
        /// <param name="collection">The TestResultCollection which will host the result.</param>
        private static void ParseTestUnitsLog(XmlNodeList nodes, QualifiedNameBuilder path, TestResultCollection collection)
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
        /// <param name="collection">The TestResultCollection which will host the result.</param>
        private static void ParseTestSuiteLog(XmlNode node, QualifiedNameBuilder path, TestResultCollection collection)
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
        /// <param name="collection">The TestResultCollection which will host the result.</param>
        private static void ParseTestCaseLog(XmlNode node, QualifiedNameBuilder path, TestResultCollection collection)
        {
            // Temporarily push TestCase on TestSuite name builder to acquire the fully qualified name of the TestCase
            path.Push(node.Attributes[Xml.Name].Value);

            // Acquire result record of this TestCase
            TestResult result = collection[path.ToString()];
            if (result == null)
            {
                result = new TestResult(collection);
                collection[path.ToString()] = result;
            }

            // Reset path to original value
            path.Pop();

            XmlNode testingTime = node.SelectSingleNode(Xml.TestingTime);

            if (testingTime != null)
            {
                // Boost test testing time is listed in microseconds
                result.Duration = ulong.Parse(testingTime.InnerText, CultureInfo.InvariantCulture);
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
                        case Xml.Info: entry = new LogEntryInfo(child.InnerText); break;
                        case Xml.Message: entry = new LogEntryMessage(child.InnerText); break;
                        case Xml.Warning: entry = new LogEntryWarning(child.InnerText); break;
                        case Xml.Error: entry = new LogEntryError(child.InnerText); break;
                        case Xml.FatalError: entry = new LogEntryFatalError(child.InnerText); break;
                        case Xml.Exception: entry = ParseTestCaseLogException(child); break;
                    }

                    if (entry != null)
                    {
                        entry.Source = ParseSourceInfo(child);
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
        /// Parse a LogException from the provided node.
        /// </summary>
        /// <param name="node">The Xml node which contains exception information.</param>
        /// <returns>A LogEntryException populated from the provided Xml node.</returns>
        private static LogEntryException ParseTestCaseLogException(XmlNode node)
        {
            LogEntryException exception = new LogEntryException();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.CDATA)
                {
                    exception.Detail = child.InnerText;
                }
                else if ((child.NodeType == XmlNodeType.Element) && (child.Name == Xml.LastCheckpoint))
                {
                    exception.LastCheckpoint = ParseSourceInfo(child);
                    exception.CheckpointDetail = child.InnerText;
                }
            }

            return exception;
        }
    }
}