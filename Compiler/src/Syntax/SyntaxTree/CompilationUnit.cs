using System.Collections.Immutable;
using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{

    internal sealed class CompilationUnit : SyntaxNode
    {
        public CompilationUnit(TextSpan span, params SyntaxNode[] nodes)
        {
            Span = span;
            Nodes = nodes.ToImmutableArray();
        }

        public override TextSpan Span { get; }
        public ImmutableArray<SyntaxNode> Nodes;

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach(var node in Nodes)
                builder.Append(node.ToString() + '\n');

            builder.Remove(builder.Length -1, 1);
            return builder.ToString();
        }
    }
}