using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class SwitchStatementSyntax : StatementSyntax
    {
        internal SwitchStatementSyntax(SyntaxToken switchToken, ExpressionSyntax expression, BlockStatmentSyntax cases, bool isValid, TextLocation location) : base(isValid, location)
        {
            SwitchToken = switchToken;
            Expression = expression;
            Cases = cases;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.SwitchStatementSyntax;

        public SyntaxToken SwitchToken { get; }
        public ExpressionSyntax Expression { get; }
        public BlockStatmentSyntax Cases { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return SwitchToken;
            yield return Expression;
            foreach (var @case in Cases.Statements)
                yield return @case;
        }
    }
}