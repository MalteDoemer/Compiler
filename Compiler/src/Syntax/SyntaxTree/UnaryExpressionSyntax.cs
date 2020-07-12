using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxToken op, ExpressionSyntax expression, bool isValid, TextLocation location)
        {
            Op = op;
            Expression = expression;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.UnaryExpressionSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"({Op.Value}{Expression})";
    }
}