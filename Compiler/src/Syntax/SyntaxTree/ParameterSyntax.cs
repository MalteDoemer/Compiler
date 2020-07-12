using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax typeClause, bool isValid = true)
        {
            Identifier = identifier;
            TypeClause = typeClause;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ParameterSyntax;
        public override TextSpan Location => Identifier.Span + TypeClause.Location;
        public override bool IsValid { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }

        public override string ToString() => $"{Identifier.Value}{TypeClause}";
    }

}