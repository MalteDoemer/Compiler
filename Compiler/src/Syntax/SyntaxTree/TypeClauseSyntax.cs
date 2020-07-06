using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxToken colonToken, SyntaxToken typeToken, bool isExplicit, bool isValid = true)
        {
            ColonToken = colonToken;
            TypeToken = typeToken;
            IsExplicit = isExplicit;
            IsValid = isValid;
        }

        public override TextSpan Span => TypeToken.Span;
        public override bool IsValid { get; }
        public bool IsExplicit { get; }
        public SyntaxToken ColonToken { get; }
        public SyntaxToken TypeToken { get; }

        public override string ToString() => $" : {TypeToken.Value}";
    }
}