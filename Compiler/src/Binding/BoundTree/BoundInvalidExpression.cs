using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public BoundInvalidExpression(TextSpan span)
        {
            Span = span;
        }

        public override TypeSymbol ResultType => TypeSymbol.NullType;

        public override TextSpan Span { get; }
    }
}