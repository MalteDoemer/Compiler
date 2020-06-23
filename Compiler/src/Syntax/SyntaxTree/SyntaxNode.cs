using Compiler.Text;

namespace Compiler.Syntax
{

    internal abstract class SyntaxNode
    {
        public abstract TextSpan Span { get; }

        public abstract override string ToString();
    }
}