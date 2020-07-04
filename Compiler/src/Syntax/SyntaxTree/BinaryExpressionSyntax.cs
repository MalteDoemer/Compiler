using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(SyntaxToken op, ExpressionSyntax left, ExpressionSyntax right, bool isValid = true)
        {
            Op = op;
            Left = left;
            Right = right;
            IsValid = isValid;
        }

        public override TextSpan Span => Left.Span + Right.Span;
        public override bool IsValid { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Left { get; }
        public ExpressionSyntax Right { get; }

        public override string ToString() => $"({Left} {Op.Value} {Right})";
    }
}