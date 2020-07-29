using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class ArrayTypeSyntax : TypeSyntax
    {
        public ArrayTypeSyntax(TypeSyntax underlyingType, SyntaxToken leftBracket, ExpressionSyntax? size, SyntaxToken rightBracket, bool isValid, TextLocation location) : base(isValid, location)
        {
            UnderlyingType = underlyingType;
            LeftBracket = leftBracket;
            Size = size;
            RightBracket = rightBracket;
        }

        public TypeSyntax UnderlyingType { get; }
        public SyntaxToken LeftBracket { get; }
        public ExpressionSyntax? Size { get; }
        public SyntaxToken RightBracket { get; }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ArrayTypeSyntax;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return UnderlyingType;
            yield return LeftBracket;
            if (!(Size is null))
                yield return Size;
            yield return RightBracket;
        }
    }

}