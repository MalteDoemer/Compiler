using System;
using System.Collections.Immutable;
using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class BlockStatment : StatementSyntax
    {
        public BlockStatment(SyntaxToken openCurly, ImmutableArray<StatementSyntax> statements, SyntaxToken closeCurly)
        {
            OpenCurly = openCurly;
            Statements = statements;
            CloseCurly = closeCurly;
        }

        public SyntaxToken OpenCurly { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseCurly { get; }
        public override TextSpan Span => OpenCurly.Span + CloseCurly.Span;
    }
}