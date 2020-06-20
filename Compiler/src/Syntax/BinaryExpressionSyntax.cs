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

        public override int Pos => Op.pos;
        public override string ToString() => $"({Left} {Op.value} {Right})";
    }
}