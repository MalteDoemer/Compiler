using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxToken op, ExpressionSyntax expression, bool isValid = true)
        {
            Op = op;
            Expression = expression;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.UnaryExpressionSyntax;
        public override TextSpan Span => Op.Span + Expression.Span;
        public override bool IsValid { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"({Op.Value}{Expression})";
    }
}