using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax typeClause, bool isValid, TextLocation location)
        {
            Identifier = identifier;
            TypeClause = typeClause;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ParameterSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }

        public override string ToString() => $"{Identifier.Value}{TypeClause}";
    }

}