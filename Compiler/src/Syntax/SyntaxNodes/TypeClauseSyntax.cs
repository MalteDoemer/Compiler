using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxToken colonToken, SyntaxToken typeToken, bool isExplicit, bool isValid, TextLocation location) : base(isValid, location)
        {
            ColonToken = colonToken;
            TypeToken = typeToken;
            IsExplicit = isExplicit;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.TypeClauseSyntax;
        public bool IsExplicit { get; }
        public SyntaxToken ColonToken { get; }
        public SyntaxToken TypeToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ColonToken;
            yield return TypeToken;
        }
    }
}