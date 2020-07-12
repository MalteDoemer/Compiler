using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class PostIncDecExpressionSyntax : ExpressionSyntax
    {
        public PostIncDecExpressionSyntax(SyntaxToken identifier, SyntaxToken op, bool isValid = true)
        {
            Identifier = identifier;
            Op = op;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.PostIncDecExpressionSyntax;
        public override TextSpan Span => Identifier.Span + Op.Span;
        public override bool IsValid { get; }

        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }

        public override string ToString() => $"({Identifier.Value}{Op.Value})";
    }
}