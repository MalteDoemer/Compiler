using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class PostIncDecExpression : ExpressionSyntax
    {
        public PostIncDecExpression(SyntaxToken identifier, SyntaxToken op)
        {
            Identifier = identifier;
            Op = op;
        }

        public override TextSpan Span => Identifier.Span + Op.Span;
        public override bool IsValid => Identifier.IsValid && Op.IsValid;

        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }

        public override string ToString() => $"({Identifier.Value}{Op.Value})";
    }
}