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

}