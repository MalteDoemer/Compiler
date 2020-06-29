using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;



namespace Compiler.Syntax
{
    internal sealed class ArgumentList : IEnumerable<ExpressionSyntax>
    {
        public ArgumentList(SyntaxToken leftParenthesis, ImmutableArray<ExpressionSyntax> arguments, ImmutableArray<SyntaxToken> commas, SyntaxToken rightParenthesis)
        {
            RightParenthesis = rightParenthesis;
            Arguments = arguments;
            Commas = commas;
            LeftParenthesis = leftParenthesis;
        }

        public SyntaxToken LeftParenthesis { get; }
        public ImmutableArray<ExpressionSyntax> Arguments { get; }
        public ImmutableArray<SyntaxToken> Commas { get; }
        public SyntaxToken RightParenthesis { get; }
        public bool IsValid
        {
            get
            {
                var valid = RightParenthesis.IsValid && LeftParenthesis.IsValid;
                foreach (var arg in Arguments)
                    valid = valid && arg.IsValid;
                foreach (var comma in Commas)
                    valid = valid && comma.IsValid;
                return valid;
            }
        }

        public ExpressionSyntax this[int index] { get => Arguments[index]; }

        public IEnumerator<ExpressionSyntax> GetEnumerator()
        {
            foreach (var arg in Arguments)
                yield return arg;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            var builder = new StringBuilder();

            for (int i = 0; i < Arguments.Length - 1; i++)
            {
                builder.Append(Arguments[i]);
                builder.Append(", ");
            }

            if (!Arguments.IsEmpty)
                builder.Append(Arguments[Arguments.Length - 1]);

            return $"({builder})";
        }

    }
}