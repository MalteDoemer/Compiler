using Compiler.Symbols;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value, TypeSymbol symbol, bool isValid)
        {
            Value = value;
            ResultType = symbol;
            IsValid = isValid;

            if (!(value is InterpolatedString))
                Constant = new BoundConstant(Value);
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundLiteralExpression;
        public override BoundConstant Constant { get; }
        public override bool IsValid { get; }
        public object Value { get; }
        public override TypeSymbol ResultType { get; }
    }
}