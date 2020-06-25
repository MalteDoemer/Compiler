using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(SyntaxToken op, ExpressionSyntax left, ExpressionSyntax right)
        {
            Op = op;
            Left = left;
            Right = right;
        }

        public SyntaxToken Op { get; }
        public ExpressionSyntax Left { get; }
        public ExpressionSyntax Right { get; }
        public override TextSpan Span => Left.Span + Right.Span;
        public override bool IsValid => Op.IsValid && Left.IsValid && Right.IsValid;

        public override string ToString()
        {
            return $"({Left} {Op.Value} {Right})";
        }
    }
}