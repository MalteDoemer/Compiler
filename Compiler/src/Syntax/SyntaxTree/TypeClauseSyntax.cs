using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxToken colonToken, SyntaxToken typeToken)
        {
            ColonToken = colonToken;
            TypeToken = typeToken;
        }

        public override TextSpan Span => TypeToken.Span;
        public override bool IsValid => ColonToken.IsValid && TypeToken.IsValid;
        public SyntaxToken ColonToken { get; }
        public SyntaxToken TypeToken { get; }

        public override string ToString() => $": {TypeToken.Value}";
    }
    
}