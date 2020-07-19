using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class PostIncDecExpressionSyntax : ExpressionSyntax
    {
        public PostIncDecExpressionSyntax(SyntaxToken identifier, SyntaxToken op, bool isValid, TextLocation location) : base(isValid, location)
        {
            Identifier = identifier;
            Op = op;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.PostIncDecExpressionSyntax;
        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }
    }
}