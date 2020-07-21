using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        internal UnaryExpressionSyntax(SyntaxToken op, ExpressionSyntax expression, bool isValid, TextLocation? location) : base(isValid, location)
        {
            Op = op;
            Expression = expression;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.UnaryExpressionSyntax;
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Op;
            yield return Expression;
        }
    }
}