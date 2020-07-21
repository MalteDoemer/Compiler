using System;
using System.Linq;
using System.Collections.Immutable;
using System.Text;
using Compiler.Text;
using System.Collections.Generic;

namespace Compiler.Syntax
{
    public sealed class BlockStatmentSyntax : StatementSyntax
    {
        internal BlockStatmentSyntax(SyntaxToken openCurly, ImmutableArray<StatementSyntax> statements, SyntaxToken closeCurly, bool isValid, TextLocation location) : base(isValid, location)
        {
            OpenCurly = openCurly;
            Statements = statements;
            CloseCurly = closeCurly;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.BlockStatmentSyntax;
        public SyntaxToken OpenCurly { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseCurly { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenCurly;
            foreach (var stmt in Statements) 
                yield return stmt;
            yield return CloseCurly;
        }
    }
}