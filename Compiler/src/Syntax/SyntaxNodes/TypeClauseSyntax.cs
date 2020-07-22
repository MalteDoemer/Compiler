using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class TypeClauseSyntax : SyntaxNode
    {
        internal TypeClauseSyntax(SyntaxToken colonToken, SyntaxToken typeToken, ImmutableArray<SyntaxToken> brackets, bool isExplicit, bool isValid, TextLocation location) : base(isValid, location)
        {
            ColonToken = colonToken;
            TypeToken = typeToken;
            Brackets = brackets;
            IsExplicit = isExplicit;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.TypeClauseSyntax;
        public bool IsExplicit { get; }
        public SyntaxToken ColonToken { get; }
        public SyntaxToken TypeToken { get; }
        public ImmutableArray<SyntaxToken> Brackets { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ColonToken;
            yield return TypeToken;
            foreach (var bracket in Brackets)
                yield return bracket;
        }
    }
}