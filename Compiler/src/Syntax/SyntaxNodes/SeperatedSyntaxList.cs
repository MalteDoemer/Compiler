using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Text;

// TODO Refactor SeperatedSyntaxList

namespace Compiler.Syntax
{
    public sealed class SeperatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode
    {
        public static readonly SeperatedSyntaxList<T> Empty = new SeperatedSyntaxList<T>(ImmutableArray<T>.Empty, ImmutableArray<SyntaxToken>.Empty, true, null);

        private readonly ImmutableArray<T> nodes;
        private readonly ImmutableArray<SyntaxToken> seperators;

        internal SeperatedSyntaxList(ImmutableArray<T> nodes, ImmutableArray<SyntaxToken> seperators, bool isValid, TextLocation? location)
        {
            this.nodes = nodes;
            this.seperators = seperators;
            IsValid = isValid;
            Location = location;
            Length = nodes.Length;
        }

        public bool IsValid { get; }
        public int Length { get; }
        public TextLocation? Location { get; }

        public T this[int index] { get => nodes[index]; }
        public ImmutableArray<T> GetNodes() => nodes;
        public ImmutableArray<SyntaxToken> GetSeperators() => seperators;

        public IEnumerator<T> GetEnumerator() { foreach (var n in nodes) yield return n; }
        IEnumerator IEnumerable.GetEnumerator() { foreach (var n in nodes) yield return n; }
    }
}