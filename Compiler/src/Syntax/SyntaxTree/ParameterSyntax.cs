using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxToken typeKeyword, SyntaxToken identifier)
        {
            TypeKeyword = typeKeyword;
            Identifier = identifier;
        }

        public override TextSpan Span => TypeKeyword.Span + Identifier.Span;
        public override bool IsValid => TypeKeyword.IsValid && Identifier.IsValid;

        public SyntaxToken TypeKeyword { get; }
        public SyntaxToken Identifier { get; }

        public override string ToString() => $"{TypeKeyword.Value} {Identifier.Value}";
    }

}