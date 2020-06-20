using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxToken op, ExpressionSyntax expression)
        {
            Op = op;
            Expression = expression;
        }

        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override TextSpan Span => Op.Span + Expression.Span;

        public override string ToString() => $"({Op.Value}{Expression})";
    }
}