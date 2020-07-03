using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Text;

namespace Compiler.Syntax
{

    internal sealed class SeperatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode
    {
        private readonly ImmutableArray<T> nodes;
        private readonly ImmutableArray<SyntaxToken> seperators;

        public SeperatedSyntaxList(ImmutableArray<T> nodes, ImmutableArray<SyntaxToken> seperators)
        {
            this.nodes = nodes;
            this.seperators = seperators;
        }

        public bool IsValid
        {
            get
            {
                var valid = true;
                foreach (var arg in nodes)
                    valid = valid && arg.IsValid;
                foreach (var comma in seperators)
                    valid = valid && comma.IsValid;
                return valid;
            }
        }

        public TextSpan Span
        {
            get
            {
                if (nodes.IsEmpty)
                    return TextSpan.Invalid;
                if (nodes.Length == 1)
                    return nodes[0].Span;

                return nodes.First().Span + nodes.Last().Span;
            }
        }

        public int Length => nodes.Length;

        public T this[int index] { get => nodes[index]; }

        public ImmutableArray<T> GetNodes() => nodes;
        public ImmutableArray<SyntaxToken> GetSeperators() => seperators;

        public IEnumerator<T> GetEnumerator() { foreach (var n in nodes) yield return n; }
        IEnumerator IEnumerable.GetEnumerator() { foreach (var n in nodes) yield return n; }
    }

    // internal sealed class ArgumentList : IEnumerable<ExpressionSyntax>
    // {
    //     public ArgumentList(SyntaxToken leftParenthesis, ImmutableArray<ExpressionSyntax> arguments, ImmutableArray<SyntaxToken> commas, SyntaxToken rightParenthesis)
    //     {
    //         RightParenthesis = rightParenthesis;
    //         Arguments = arguments;
    //         Commas = commas;
    //         LeftParenthesis = leftParenthesis;
    //     }

    //     public SyntaxToken LeftParenthesis { get; }
    //     public ImmutableArray<ExpressionSyntax> Arguments { get; }
    //     public ImmutableArray<SyntaxToken> Commas { get; }
    //     public SyntaxToken RightParenthesis { get; }
    //     public bool IsValid
    //     {
    //         get
    //         {
    //             var valid = RightParenthesis.IsValid && LeftParenthesis.IsValid;
    //             foreach (var arg in Arguments)
    //                 valid = valid && arg.IsValid;
    //             foreach (var comma in Commas)
    //                 valid = valid && comma.IsValid;
    //             return valid;
    //         }
    //     }

    //     public ExpressionSyntax this[int index] { get => Arguments[index]; }

    //     public IEnumerator<ExpressionSyntax> GetEnumerator()
    //     {
    //         foreach (var arg in Arguments)
    //             yield return arg;
    //     }

    //     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    //     public override string ToString()
    //     {
    //         var builder = new StringBuilder(15);

    //         for (int i = 0; i < Arguments.Length - 1; i++)
    //         {
    //             builder.Append(Arguments[i]);
    //             StringBuilder stringBuilder = builder.Append(", ");
    //         }

    //         if (!Arguments.IsEmpty)
    //             builder.Append(Arguments[Arguments.Length - 1]);

    //         return $"({builder})";
    //     }

    // }
}