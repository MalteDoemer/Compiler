using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literal, bool isValid, TextLocation location) : base(isValid, location)
        {
            Literal = literal;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.LiteralExpressionSyntax;
        public SyntaxToken Literal { get; }
    }
}