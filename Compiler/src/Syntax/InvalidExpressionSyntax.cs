namespace Compiler.Syntax
{
    internal sealed class InvalidExpressionSyntax : ExpressionSyntax
    {
        public InvalidExpressionSyntax(SyntaxToken invalidToken)
        {
            InvalidToken = invalidToken;
        }

        public override int Pos => InvalidToken.pos;

        public SyntaxToken InvalidToken { get; }
    }
}