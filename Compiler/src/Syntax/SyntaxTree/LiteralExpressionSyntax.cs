using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literal)
        {
            Literal = literal;
        }

        public SyntaxToken Literal { get; }

        public override TextSpan Span => Literal.Span;

        public override bool IsValid => Literal.IsValid;

        public override string ToString()
        {
            return Literal.Value.ToString();
        }
    }
}