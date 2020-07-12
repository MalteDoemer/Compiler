using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literal, bool isValid, TextLocation location)
        {
            Literal = literal;
            IsValid = isValid;
            Location = location;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.LiteralExpressionSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken Literal { get; }

        public override string ToString() => Literal.Value.ToString();
    }
}