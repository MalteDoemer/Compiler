namespace Compiler.Syntax
{
    internal sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literal)
        {
            Literal = literal;
        }

        public SyntaxToken Literal { get; }
        public override int Pos => Literal.pos;

        public override string ToString() => $"{Literal.value}";
    }
}