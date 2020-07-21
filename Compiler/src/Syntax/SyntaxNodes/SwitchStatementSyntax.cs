using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class SwitchStatementSyntax : StatementSyntax
    {
        internal SwitchStatementSyntax(SyntaxToken switchToken, ExpressionSyntax expression, SyntaxToken leftCuryl, ImmutableArray<CaseStatementSyntax> cases, SyntaxToken rightCuryl, bool isValid, TextLocation location) : base(isValid, location)
        {
            SwitchToken = switchToken;
            Expression = expression;
            LeftCuryl = leftCuryl;
            Cases = cases;
            RightCuryl = rightCuryl;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.SwitchStatementSyntax;

        public SyntaxToken SwitchToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken LeftCuryl { get; }
        public ImmutableArray<CaseStatementSyntax> Cases { get; }
        public SyntaxToken RightCuryl { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return SwitchToken;
            yield return Expression;
            yield return LeftCuryl;
            foreach (var @case in Cases)
                yield return @case;
            yield return RightCuryl;
        }
    }
}