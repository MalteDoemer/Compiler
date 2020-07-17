using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(SyntaxToken op, ExpressionSyntax left, ExpressionSyntax right, bool isValid, TextLocation location) : base(isValid, location)
        {
            Op = op;
            Left = left;
            Right = right;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.BinaryExpressionSyntax;
        public SyntaxToken Op { get; }
        public ExpressionSyntax Left { get; }
        public ExpressionSyntax Right { get; }

        public override string ToString() => $"({Left} {Op.Value} {Right})";
    }
}