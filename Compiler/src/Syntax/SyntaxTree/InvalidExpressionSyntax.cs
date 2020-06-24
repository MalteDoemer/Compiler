using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class InvalidExpressionSyntax : ExpressionSyntax
    {
        public InvalidExpressionSyntax(TextSpan span)
        {
            Span = span;
        }

        public override TextSpan Span { get; }

        public override string ToString()
        {
            return "(Invalid)";
        }
    }
}