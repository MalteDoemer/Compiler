using System.Collections.Immutable;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement(TextSpan lCurlySpan, ImmutableArray<BoundStatement> statements, TextSpan rCurylSpan)
        {
            LCurlySpan = lCurlySpan;
            Statements = statements;
            RCurylSpan = rCurylSpan;
        }


        public TextSpan LCurlySpan { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
        public TextSpan RCurylSpan { get; }
        public override TextSpan Span => LCurlySpan + RCurylSpan;
    }
}