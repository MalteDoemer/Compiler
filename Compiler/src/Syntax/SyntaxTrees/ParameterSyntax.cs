using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax typeClause, bool isValid, TextLocation location) : base(isValid, location)
        {
            Identifier = identifier;
            TypeClause = typeClause;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ParameterSyntax;
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }

        public override string ToString() => $"{Identifier.Value}{TypeClause}";
    }

}