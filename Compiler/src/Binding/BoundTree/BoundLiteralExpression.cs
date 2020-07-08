using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value, TypeSymbol symbol)
        {
            Value = value;
            ResultType = symbol;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundLiteralExpression;
        public object Value { get; }
        public override TypeSymbol ResultType { get; }

        public override string ToString() => $"({Value})";
    }
}