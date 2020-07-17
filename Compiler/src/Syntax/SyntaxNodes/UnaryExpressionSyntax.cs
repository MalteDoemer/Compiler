using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxToken op, ExpressionSyntax expression, bool isValid, TextLocation location) : base(isValid, location)
        {
            Op = op;
            Expression = expression;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.UnaryExpressionSyntax;
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"({Op.Value}{Expression})";
    }
}