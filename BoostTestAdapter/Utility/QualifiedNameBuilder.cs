// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using BoostTestAdapter.Boost.Test;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Builds qualified names for Boost Test Test Units.
    /// </summary>
    public class QualifiedNameBuilder : ICloneable
    {
        #region Constants

        private const string Separator = "/";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public QualifiedNameBuilder()
            : this(new List<string>())
        {
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="path">The initial fully qualified path</param>
        private QualifiedNameBuilder(List<string> path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Constructor. Initializes this qualified name based on the provided TestUnit.
        /// </summary>
        /// <param name="root">The TestUnit from which this qualified name is to be initialized.</param>
        public QualifiedNameBuilder(TestUnit root) :
            this()
        {
            Initialize(root);
        }

        #endregion Constructors

        #region Helper Methods

        /// <summary>
        /// Helper function which aids in the implementation of QualifiedNameBuilder(TestUnit) constructor
        /// </summary>
        /// <param name="root">The test unit which is to be listed</param>
        private void Initialize(TestUnit root)
        {
            if (root == null)
            {
                return;
            }

            Initialize(root.Parent);

            this.Push(root);
        }

        #endregion Helper Methods

        #region Properties

        /// <summary>
        /// The Master Test Suite local name.
        /// </summary>
        public string MasterTestSuite
        {
            get { return this.Path.FirstOrDefault(); }
        }

        /// <summary>
        /// Stack which contains the entries for this qualified name.
        /// </summary>
        private List<string> Path { get; set; }

        /// <summary>
        /// The depth of this fully qualified name.
        /// A depth of 0 implies an empty object.
        /// </summary>
        public int Level { get { return this.Path.Count; } }

        #endregion Properties

        #region Constant Properties
        
        /// <summary>
        /// Identifies the default MasterTestSuite test suite name.
        /// </summary>
        public static string DefaultMasterTestSuiteName { get { return "Master Test Suite"; } }

        #endregion Constant Properties

        /// <summary>
        /// Pushes the test unit on this structure.
        /// </summary>
        /// <param name="unit">The test unit to push</param>
        /// <returns>this</returns>
        public QualifiedNameBuilder Push(TestUnit unit)
        {
            Utility.Code.Require(unit, "unit");

            return this.Push(unit.Name);
        }

        /// <summary>
        /// Pushes the (local) name of a test unit on this structure.
        /// </summary>
        /// <param name="name">The test unit (local) name to push</param>
        /// <returns>this</returns>
        public QualifiedNameBuilder Push(string name)
        {
            this.Path.Add(name);

            return this;
        }

        /// <summary>
        /// Peeks at the last (local) name pushed on this builder.
        /// </summary>
        /// <returns>The last (local) name pushed on this builder.</returns>
        public string Peek()
        {
            return (this.Path.Count > 0) ? this.Path[this.Path.Count - 1] : null;
        }

        /// <summary>
        /// Pops the last test unit from this instance.
        /// </summary>
        /// <returns>this</returns>
        public QualifiedNameBuilder Pop()
        {
            if (this.Path.Count > 0)
            {
                this.Path.RemoveAt(this.Path.Count - 1);
            }

            return this;
        }

        #region object overrides

        /// <summary>
        /// Provides a string representation of this fully qualified name as expected by Boost Test standards.
        /// </summary>
        /// <returns>A string representation of this fully qualified name as expected by Boost Test standards.</returns>
        public override string ToString()
        {
            // Skip the Master Test Suite. Master Test Suite is omitted in qualified name.
            return string.Join(Separator, this.Path.Skip(1));
        }

        #endregion object overrides

        /// <summary>
        /// Factory method which creates a QualifiedNameBuilder
        /// from an already existing qualified name string.
        /// </summary>
        /// <param name="name">The qualified name</param>
        /// <returns>A QualifiedNameBuilder from the provided string.</returns>
        public static QualifiedNameBuilder FromString(string name)
        {
            // Assume Master Test Suite name
            return FromString(DefaultMasterTestSuiteName, name);
        }

        /// <summary>
        /// Factory method which creates a QualifiedNameBuilder
        /// from an already existing qualified name string.
        /// </summary>
        /// <param name="masterSuite">The local name of the master test suite</param>
        /// <param name="name">The qualified name</param>
        /// <returns>A QualifiedNameBuilder from the provided string.</returns>
        public static QualifiedNameBuilder FromString(string masterSuite, string name)
        {
            Utility.Code.Require(masterSuite, "masterSuite");
            Utility.Code.Require(name, "name");

            QualifiedNameBuilder builder = new QualifiedNameBuilder();

            builder.Push(masterSuite);

            foreach (string part in name.Split(new string[] { Separator }, StringSplitOptions.RemoveEmptyEntries))
            {
                builder.Push(part);
            }

            return builder;
        }

        #region ICloneable

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public QualifiedNameBuilder Clone()
        {
            // NOTE Make an explicit copy of the path
            return new QualifiedNameBuilder(new List<string>(this.Path));
        }

        #endregion ICloneable
    }
}