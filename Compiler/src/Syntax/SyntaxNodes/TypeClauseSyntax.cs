using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class TypeClauseSyntax : SyntaxNode
    {
        internal TypeClauseSyntax(SyntaxToken colonToken, TypeSyntax typeSyntax, bool isExplicit, bool isValid, TextLocation location) : base(isValid, location)
        {
            ColonToken = colonToken;
            TypeSyntax = typeSyntax;
            IsExplicit = isExplicit;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.TypeClauseSyntax;
        public bool IsExplicit { get; }
        public SyntaxToken ColonToken { get; }
        public TypeSyntax TypeSyntax { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ColonToken;
            yield return TypeSyntax;
        }
    }

    public abstract class TypeSyntax : SyntaxNode
    {
        protected TypeSyntax(bool isValid, TextLocation location) : base(isValid, location)
        {
        }
    }

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

    public sealed class ArrayTypeSyntax : TypeSyntax
    {
        public ArrayTypeSyntax(TypeSyntax underlyingType, SyntaxToken leftBracket, ExpressionSyntax size, SyntaxToken rightBracket, bool isValid, TextLocation location) : base(isValid, location)
        {
            UnderlyingType = underlyingType;
            LeftBracket = leftBracket;
            Size = size;
            RightBracket = rightBracket;
        }

        public TypeSyntax UnderlyingType { get; }
        public SyntaxToken LeftBracket { get; }
        public ExpressionSyntax Size { get; }
        public SyntaxToken RightBracket { get; }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ArrayTypeSyntax;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return UnderlyingType;
            yield return LeftBracket;
            if (Size is not null)
                yield return Size;
            yield return RightBracket;
        }
    }

}