using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(TypeSymbol type, BoundExpression expression, bool isValid)
        {
            Type = type;
            Expression = expression;
            IsValid = isValid;
        }

        public override TypeSymbol ResultType => Type;

        public TypeSymbol Type { get; }
        public BoundExpression Expression { get; }

        public override bool IsValid { get; }
    }
}