using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class PreDefinedTypeSyntax : TypeSyntax
    {
        public PreDefinedTypeSyntax(SyntaxToken typeToken, bool isValid, TextLocation location) : base(isValid, location)
        {
            TypeToken = typeToken;
        }

        public SyntaxToken TypeToken { get; }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.PreDefinedTypeSyntax;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return TypeToken;
        }
    }

}