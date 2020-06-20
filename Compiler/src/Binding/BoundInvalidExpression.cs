using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public override TypeSymbol ResultType => TypeSymbol.NullType;

        public override TextSpan Span => TextSpan.Invalid;
    }
}