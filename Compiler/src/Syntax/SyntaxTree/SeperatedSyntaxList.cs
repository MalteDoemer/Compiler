using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Text;

namespace Compiler.Syntax
{

    internal sealed class SeperatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode
    {
        public static readonly SeperatedSyntaxList<T> Empty = new SeperatedSyntaxList<T>(ImmutableArray<T>.Empty, ImmutableArray<SyntaxToken>.Empty);

        private readonly ImmutableArray<T> nodes;
        private readonly ImmutableArray<SyntaxToken> seperators;

        public SeperatedSyntaxList(ImmutableArray<T> nodes, ImmutableArray<SyntaxToken> seperators, bool isValid = true)
        {
            this.nodes = nodes;
            this.seperators = seperators;
            IsValid = isValid;
            Length = nodes.Length;

            if (nodes.IsEmpty)
                Span = TextSpan.Undefined;
            else if (nodes.Length == 1)
                Span = nodes[0].Span;
            else 
                Span = nodes.First().Span + nodes.Last().Span;
        }

        public bool IsValid { get; }
        public int Length { get; }
        public TextSpan Span { get; }

        public T this[int index] { get => nodes[index]; }
        public ImmutableArray<T> GetNodes() => nodes;
        public ImmutableArray<SyntaxToken> GetSeperators() => seperators;

        public IEnumerator<T> GetEnumerator() { foreach (var n in nodes) yield return n; }
        IEnumerator IEnumerable.GetEnumerator() { foreach (var n in nodes) yield return n; }
    }
}