using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literal, bool isValid = true)
        {
            Literal = literal;
            IsValid = isValid;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.LiteralExpressionSyntax;
        public override TextSpan Location => Literal.Span;
        public override bool IsValid { get; }
        public SyntaxToken Literal { get; }

        public override string ToString() => Literal.Value.ToString();
    }
}