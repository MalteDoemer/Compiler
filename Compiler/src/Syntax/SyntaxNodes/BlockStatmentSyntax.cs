using System;
using System.Linq;
using System.Collections.Immutable;
using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class BlockStatmentSyntax : StatementSyntax
    {
        public BlockStatmentSyntax(SyntaxToken openCurly, ImmutableArray<StatementSyntax> statements, SyntaxToken closeCurly, bool isValid, TextLocation location) : base(isValid, location)
        {
            OpenCurly = openCurly;
            Statements = statements;
            CloseCurly = closeCurly;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.BlockStatmentSyntax;
        public SyntaxToken OpenCurly { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseCurly { get; }
    }
}