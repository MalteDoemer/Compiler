using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public BoundInvalidExpression(TextSpan span)
        {
            Span = span;
        }

        public override TypeSymbol ResultType => TypeSymbol.Invalid;

        public override TextSpan Span { get; }
    }
}