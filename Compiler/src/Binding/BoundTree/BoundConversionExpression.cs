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
        public override BoundNodeKind Kind => BoundNodeKind.BoundConversionExpression;
        public override TypeSymbol ResultType => Type;
        public override bool IsValid { get; set; }
        public TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }
}