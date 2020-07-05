using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value, TypeSymbol symbol, bool isValid)
        {
            Value = value;
            ResultType = symbol;
            IsValid = isValid;
        }
        
        public object Value { get; }
        public override TypeSymbol ResultType { get; }

        public override bool IsValid { get; }
    }
}