using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class PostIncDecExpressionSyntax : ExpressionSyntax
    {
        public PostIncDecExpressionSyntax(SyntaxToken identifier, SyntaxToken op, bool isValid, TextLocation location)
        {
            Identifier = identifier;
            Op = op;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.PostIncDecExpressionSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }

        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }

        public override string ToString() => $"({Identifier.Value}{Op.Value})";
    }
}