using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Compiler.Text;



namespace Compiler.Syntax
{

    internal abstract class SyntaxNode
    {
        public abstract TextSpan Span { get; }
        public abstract bool IsValid { get; }

        public abstract override string ToString();

    }

    internal sealed class ArgumentList : IEnumerable<ExpressionSyntax>
    {
        public ArgumentList(SyntaxToken rightParenthesis, ImmutableArray<ExpressionSyntax> arguments, ImmutableArray<SyntaxToken> commas, SyntaxToken leftParenthesis)
        {
            RightParenthesis = rightParenthesis;
            Arguments = arguments;
            Commas = commas;
            LeftParenthesis = leftParenthesis;
        }

        public SyntaxToken RightParenthesis { get; }
        public ImmutableArray<ExpressionSyntax> Arguments { get; }
        public ImmutableArray<SyntaxToken> Commas { get; }
        public SyntaxToken LeftParenthesis { get; }
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

    internal sealed class CallExpressionSyntax : ExpressionSyntax
    {
        public CallExpressionSyntax(SyntaxToken identifier, ArgumentList arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }

        public override TextSpan Span => Identifier.Span + Arguments.RightParenthesis.Span;
        public override bool IsValid => Identifier.IsValid && Arguments.IsValid;

        public SyntaxToken Identifier { get; }
        public ArgumentList Arguments { get; }

        public override string ToString() => Identifier.ToString() + Arguments.ToString();
    }
}