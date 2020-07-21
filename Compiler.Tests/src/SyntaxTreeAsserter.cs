using Compiler.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Compiler.Test
{
    public class SyntaxTreeAsserter : IDisposable
    {
        private readonly IEnumerator<SyntaxNode> enumerator;
        private bool hasErrors;

        public SyntaxTreeAsserter(SyntaxNode tree)
        {
            enumerator = Flatten(tree).GetEnumerator();
        }

        private IEnumerable<SyntaxNode> Flatten(SyntaxNode root)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                yield return n;

                foreach (var child in n.GetChildren().Reverse())
                    stack.Push(child);
            }

        }

        private bool MarkFailed()
        {
            hasErrors = true;
            return false;
        }

        public void Dispose()
        {
            if (!hasErrors)
                Assert.False(enumerator.MoveNext());

            enumerator.Dispose();
        }

        public void AssertNode(SyntaxNodeKind kind)
        {
            try
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(kind, enumerator.Current.Kind);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertToken(SyntaxTokenKind kind, string text)
        {
            try
            {
                Assert.True(enumerator.MoveNext());
                var token = Assert.IsType<SyntaxToken>(enumerator.Current);
                Assert.Equal(kind, token.TokenKind);
                Assert.Equal(text, token.Location.ToString());
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }
    }
}