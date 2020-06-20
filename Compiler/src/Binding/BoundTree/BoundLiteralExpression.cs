using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(TextSpan span, dynamic value, TypeSymbol symbol)
        {
            Span = span;
            Value = value;
            ResultType = symbol;
        }
        
        public dynamic Value { get; }
        public override TypeSymbol ResultType { get; }
        public override TextSpan Span { get; }
    }
}