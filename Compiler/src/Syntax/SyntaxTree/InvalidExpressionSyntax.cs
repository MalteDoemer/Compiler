using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class InvalidExpressionSyntax : ExpressionSyntax
    {
        public InvalidExpressionSyntax(SyntaxToken invalidToken)
        {
            InvalidToken = invalidToken;
        }

    
        public SyntaxToken InvalidToken { get; }

        public override TextSpan Span => InvalidToken.Span;
    }
}