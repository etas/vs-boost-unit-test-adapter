using System;
using System.IO;

using Antlr4.Runtime;

[assembly: CLSCompliant(false)]
namespace Antlr.DOT
{
    // Reference: https://github.com/antlr/grammars-v4/blob/master/dot/DOT.g4
    // Reference: https://github.com/antlr/antlr4/blob/master/doc/listeners.md

    public static class DOT
    {
        /// <summary>
        /// Parses a DOT representation from the provided stream. The parse tree is then notified via the provided listener.
        /// </summary>
        /// <param name="stream">The stream from which to parse</param>
        /// <param name="listener">The listener which is notified of the abstract syntax nodes</param>
        public static T Parse<T>(Stream stream, IDOTVisitor<T> visitor)
        {
            var instream = new AntlrInputStream(stream);
            var lexer = new DOTLexer(instream);
            var tokenstream = new CommonTokenStream(lexer);
            var parser = new DOTParser(tokenstream);

            var graph = parser.graph();

            return visitor.Visit(graph);
        }
    }
}
