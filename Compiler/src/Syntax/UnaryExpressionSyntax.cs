namespace Compiler.Syntax
{
    internal sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxToken op, ExpressionSyntax expression)
        {
            Op = op;
            Expression = expression;
        }

        public override int Pos => Op.pos;

        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }
        public override string ToString() => $"({Op.value}{Expression})";
    }
}