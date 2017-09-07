// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using Antlr.DOT;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;

using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Test
{
    /// <summary>
    /// A Test Framework deserialiser which generates a 
    /// Test Framework instance from a Boost Test DOT representation.
    /// </summary>
    public class TestFrameworkDOTDeserialiser
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The test module source file path</param>
        public TestFrameworkDOTDeserialiser(string source)
        {
            this.Source = source;
        }

        /// <summary>
        /// The test module source file path
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Parses the stream containing a Boost Test DOT representation of a Test Framework
        /// </summary>
        /// <param name="stream">The stream consisting of a DOT representation</param>
        /// <returns>The deserialised Test Framework</returns>
        public TestFramework Deserialise(Stream stream)
        {
            BoostTestFrameworkVisitor dotVisitor = new BoostTestFrameworkVisitor(this);
            return DOT.Parse(stream, dotVisitor);
        }

        /// <summary>
        /// Parses the stream containing a Boost Test DOT representation of a Test Framework
        /// </summary>
        /// <param name="stream">The text reader consisting of a DOT representation</param>
        /// <returns>The deserialised Test Framework</returns>
        public TestFramework Deserialise(TextReader stream)
        {
            BoostTestFrameworkVisitor dotVisitor = new BoostTestFrameworkVisitor(this);
            return DOT.Parse(stream, dotVisitor);
        }

        /// <summary>
        /// Implementation of DOTBaseVisitor which creates/populates a
        /// TestFramework instance from a DOT abstract syntax tree.
        /// </summary>
        private class BoostTestFrameworkVisitor : DOTBaseVisitor<TestFramework>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parent">The parent TestFrameworkDOTDeserialiser instance</param>
            public BoostTestFrameworkVisitor(TestFrameworkDOTDeserialiser parent)
            {
                this.Parent = parent;

                this.Framework = null;
                this.Context = null;
            }

            /// <summary>
            /// The parent TestFrameworkDOTDeserialiser instance
            /// </summary>
            public TestFrameworkDOTDeserialiser Parent { get; private set; }
            
            /// <summary>
            /// The generated Test Framework
            /// </summary>
            public TestFramework Framework { get; private set; }

            /// <summary>
            /// Context class used to aggregate information during parsing
            /// </summary>
            private class DOTContext
            {
                /// <summary>
                /// Default Constructor
                /// </summary>
                public DOTContext()
                {
                    this.TestUnits = new Stack<TestUnitInfo>();
                }

                /// <summary>
                /// The current parent test suite which will host child test cases
                /// </summary>
                public TestSuite ParentSuite { get; set; }

                /// <summary>
                /// The master test suite
                /// </summary>
                public TestSuite MasterTestSuite { get; set; }

                /// <summary>
                /// In-progress test unit information which is currently being parsed
                /// </summary>
                public Stack<TestUnitInfo> TestUnits { get; private set; }
            }

            /// <summary>
            /// The current deserialisation context
            /// </summary>
            private DOTContext Context { get; set; }

            #region DOTBaseVisitor<TestFramework>

            public override TestFramework VisitGraph([NotNull] DOTParser.GraphContext context)
            {
                this.Context = new DOTContext();
                this.Framework = null;

                // Visit children
                this.Framework = base.VisitGraph(context);

                this.Framework = new TestFramework(this.Parent.Source, this.Context.MasterTestSuite);
                this.Context = null;

                return this.Framework;
            }

            public override TestFramework VisitSubgraph([NotNull] DOTParser.SubgraphContext context)
            {
                TestUnitInfo info = this.Context.TestUnits.Peek();

                TestSuite suite = CreateTestSuite(info, this.Context.ParentSuite);
                this.Context.ParentSuite = suite;

                if (this.Context.MasterTestSuite == null)
                {
                    this.Context.MasterTestSuite = suite;
                }

                // Visit Children
                this.Framework = base.VisitSubgraph(context);

                // Register any child test cases
                while (info != this.Context.TestUnits.Peek())
                {
                    CreateTestCase(this.Context.TestUnits.Pop(), this.Context.ParentSuite);
                }
                
                this.Context.TestUnits.Pop();
                this.Context.ParentSuite = (TestSuite)suite.Parent;

                return this.Framework;
            }

            public override TestFramework VisitNode_stmt([NotNull] DOTParser.Node_stmtContext context)
            {
                TestUnitInfo info = new TestUnitInfo(context.node_id().GetText());

                foreach (var attribute in GetKeyValuePairs(context.attr_list()))
                {
                    switch (attribute.Key)
                    {
                        case "color":
                            {
                                // 'green' implies that the test is explicitly enabled by default
                                // 'yellow' implies that it is enabled, but *may* be disabled
                                info.DefaultEnabled = (attribute.Value == "green");
                                break;
                            }

                        case "label":
                            {
                                // Parse BOOST Test specific content
                                info.Parse(attribute.Value.Trim('"'));
                                break;
                            }
                    }
                };

                this.Context.TestUnits.Push(info);
           
                return base.VisitNode_stmt(context);
            }

            public override TestFramework VisitEdge_stmt([NotNull] DOTParser.Edge_stmtContext context)
            {
                TestUnitInfo info = this.Context.TestUnits.Peek();

                if (info != null)
                {
                    var lhs = context.node_id();
                    var rhs = lhs;

                    var edgeRhs = context.edgeRHS();

                    // NOTE Boost Test DOT output only define one edge per edge statement
                    if (edgeRhs.edgeop().Length == 1)
                    {
                        var edgeop = edgeRhs.edgeop()[0];
                        // Ensure that a directed edge '->' token is used
                        if (edgeop.GetToken(DOTLexer.T__7, 0) != null)
                        {
                            rhs = edgeRhs.node_id()[0];
                        }
                    }

                    if ((lhs != rhs) && (rhs != null))
                    {
                        // Identify whether this edge is a constraining edge (i.e. an actual graph edge) or a non-constraining edge
                        bool constraint = !GetKeyValuePairs(context.attr_list()).Any((attribute) => (attribute.Key == "constraint") && (attribute.Value == "false"));
                        
                        // This implies a test dependency
                        if ((lhs.GetText() == info.id) && !constraint)
                        {
                            info.Dependencies.Add(rhs.GetText());
                        }
                        // This implies a test unit relationship
                        else if (rhs.GetText() == info.id)
                        {
                            info.Parents.Add(lhs.GetText());
                        }
                    }
                }

                return base.VisitEdge_stmt(context);
            }

            #endregion DOTBaseVisitor<TestFramework>

            /// <summary>
            /// Iterates over all key-value pair attributes contained within the provided attr_list
            /// </summary>
            /// <param name="attr_list">The attr_list to iterate</param>
            /// <returns>An enumeration of all key-value pairs present in the provided attr_list</returns>
            private static IEnumerable<KeyValuePair<string, string>> GetKeyValuePairs(DOTParser.Attr_listContext attr_list)
            {
                // NOTE Refer to DOT grammar; an 'attr_list' is composed of an 'a_list', which is composed of multiple 'id's

                if (attr_list != null)
                {
                    var a_lists = attr_list.a_list();
                    if (a_lists != null)
                    {
                        for (int i = 0; i < a_lists.Length; ++i)
                        {
                            var a_list = a_lists[i];

                            int id = 0;
                            int idCount = a_list.id().Length;

                            // Try to identify the id() list as a list of 'id' = 'id' tuples

                            while (id < idCount)
                            {
                                var lhs = a_list.id()[id];

                                // Reference: Antlr4 C# Runtime [ParserRuleContext.GetTokens(int)]
                                var sibling = a_list.GetChild(a_list.children.IndexOf(lhs) + 1) as ITerminalNode;

                                // Identify if the a_list contains a key/value pair separated by the '=' token
                                if ((sibling != null) && (sibling.Symbol.Type == DOTLexer.T__3))
                                {
                                    yield return new KeyValuePair<string, string>(lhs.GetText(), a_list.id()[id + 1].GetText());
                                    id += 2;
                                }
                                // Else provide the identifier a single item
                                else
                                {
                                    yield return new KeyValuePair<string, string>(lhs.GetText(), null);
                                    ++id;
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Creates a TestSuite instance from the provided TestUnitInfo structure
            /// </summary>
            /// <param name="info">The currently accumulated test unit information</param>
            /// <param name="parent">The parent test suite of this test unit</param>
            /// <returns>A TestSuite based on the provided information</returns>
            private static TestSuite CreateTestSuite(TestUnitInfo info, TestSuite parent)
            {
                return PopulateTestUnit(info, new TestSuite(info.Name, parent));
            }

            /// <summary>
            /// Creates a TestCase instance from the provided TestUnitInfo structure
            /// </summary>
            /// <param name="info">The currently accumulated test unit information</param>
            /// <param name="parent">The parent test suite of this test unit</param>
            /// <returns>A TestCase based on the provided information</returns>
            private static TestCase CreateTestCase(TestUnitInfo info, TestSuite parent)
            {
                return PopulateTestUnit(info, new TestCase(info.Name, parent));
            }

            /// <summary>
            /// Populates the provided TestUnit with the test unit information
            /// </summary>
            /// <typeparam name="T">A TestUnit derived type</typeparam>
            /// <param name="info">The currently accumulated test unit information</param>
            /// <param name="unit">The test unit instance which is to be populated</param>
            /// <returns>unit</returns>
            private static T PopulateTestUnit<T>(TestUnitInfo info, T unit) where T : TestUnit
            {
                if ((!string.IsNullOrEmpty(info.id)) && (info.id.Length > 2))
                {
                    // Remove the 'tu' prefix from the test unit string ID

                    int id = 0;
                    if (int.TryParse(info.id.Substring(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                    {
                        unit.Id = id;
                    }
                }

                unit.Source = info.SourceInfo;
                unit.Labels = info.Labels;

                unit.DefaultEnabled = info.DefaultEnabled;   

                // Default Enabled
                // Timeout
                // Expected Failures
                // Dependencies

                return unit;
            }

            /// <summary>
            /// Aggregation of test unit information contained within a Boost Test DOT serialisation
            /// </summary>
            private class TestUnitInfo
            {
                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="id">Test Unit ID (e.g. tu1)</param>
                public TestUnitInfo(string id)
                {
                    this.id = id;

                    this.Name = string.Empty;
                    this.SourceInfo = null;
                    this.Timeout = 0;
                    this.ExpectedFailures = 0;
                    this.Labels = Enumerable.Empty<string>();
                    
                    this.Parents = new List<string>();
                    this.Dependencies = new List<string>();

                    this.DefaultEnabled = true;
                }

                /// <summary>
                /// Test Unit ID (e.g. tu1)
                /// </summary>
                public string id { get; private set; }

                /// <summary>
                /// Test Unit Name
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Source information
                /// </summary>
                public SourceFileInfo SourceInfo { get; set; }

                /// <summary>
                /// Test timeout
                /// </summary>
                public uint Timeout { get; set; }

                /// <summary>
                /// Test expected failure count
                /// </summary>
                public uint ExpectedFailures { get; set; }

                /// <summary>
                /// Test labels
                /// </summary>
                public IEnumerable<string> Labels { get; set; }

                /// <summary>
                /// Test unit children
                /// </summary>
                public List<string> Parents { get; set; }

                /// <summary>
                /// Test unit dependencies
                /// </summary>
                public List<string> Dependencies { get; set; }

                /// <summary>
                /// Flag which is raised when the test is explicitly set to true
                /// </summary>
                public bool DefaultEnabled { get; set; }

                /// <summary>
                /// Parses test unit information from the provided string
                /// </summary>
                /// <param name="value">The string to parse</param>
                /// <returns>Test unit information contained within value</returns>
                /// <exception cref="FormatException"></exception>
                public TestUnitInfo Parse(string value)
                {
                    TestUnitInfo info = new TestUnitInfo(this.id);
                    info.DefaultEnabled = this.DefaultEnabled;
                    string[] properties = value.Split('|');

                    if (properties.Length > 0)
                    {
                        info.Name = properties[0];
                    }
                    if (properties.Length > 1)
                    {
                        info.SourceInfo = SourceFileInfo.Parse(properties[1]);
                    }
                    if (properties.Length > 2)
                    {
                        foreach (var attribute in ParseKeyValuePairs(properties.Skip(2)))
                        {
                            ParseNamedAttribute(info, attribute);
                        }
                    }

                    // Replace the contents of 'this' with that of 'info'
                    Set(info);

                    return this;
                }

                /// <summary>
                /// Sets the current instance properties to the one provided
                /// </summary>
                /// <param name="value"></param>
                private void Set(TestUnitInfo value)
                {
                    this.id = value.id;
                    this.Name = value.Name;
                    this.SourceInfo = value.SourceInfo;
                    this.DefaultEnabled = value.DefaultEnabled;
                    this.Timeout = value.Timeout;
                    this.ExpectedFailures = value.ExpectedFailures;
                    this.Labels = value.Labels;
                    this.Parents = value.Parents;
                    this.Dependencies = value.Dependencies;
                }

                /// <summary>
                /// Parses a Boost Test DOT named attribute label
                /// </summary>
                /// <param name="info">The test unit information structure which to populate</param>
                /// <param name="attribute">The attribute to parse</param>
                private static void ParseNamedAttribute(TestUnitInfo info, KeyValuePair<string, string> attribute)
                {
                    switch (attribute.Key)
                    {
                        case "timeout":
                            {
                                info.Timeout = uint.Parse(attribute.Value, CultureInfo.InvariantCulture);
                                break;
                            }
                        case "expected failures":
                            {
                                info.ExpectedFailures = uint.Parse(attribute.Value, CultureInfo.InvariantCulture);
                                break;
                            }
                        case "labels":
                            {
                                var labels = attribute.Value.Split(new[] { " @" }, StringSplitOptions.RemoveEmptyEntries);
                                if (labels.Length > 0)
                                {
                                    info.Labels = labels;
                                }
                                break;
                            }
                    }
                }

                /// <summary>
                /// Parses the Boost Test DOT label attributes for any key-value pairs
                /// </summary>
                /// <param name="values">The values/attributes to parse</param>
                /// <returns>An enumeration of successfully parsed key-value pairs</param>
                private static IEnumerable<KeyValuePair<string, string>> ParseKeyValuePairs(IEnumerable<string> values)
                {
                    foreach (string value in values)
                    {
                        // NOTE 'timeout' and 'expected failures' use '=' as a separator, 'labels' use ':'
                        string[] keyValue = value.Split(new[] { '=', ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (keyValue.Length == 2)
                        {
                            yield return new KeyValuePair<string, string>(keyValue[0], keyValue[1]);
                        }
                    }
                }
            }
        }
    }
}