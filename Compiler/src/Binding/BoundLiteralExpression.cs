using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        private readonly TextSpan span;
        private readonly TypeSymbol symbol;

        public BoundLiteralExpression(TextSpan span, dynamic value, TypeSymbol symbol)
        {
            this.span = span;
            Value = value;
            this.symbol = symbol;
        }

        public override TypeSymbol ResultType => symbol;
        public dynamic Value { get; }

        public override TextSpan Span => span;
    }
}