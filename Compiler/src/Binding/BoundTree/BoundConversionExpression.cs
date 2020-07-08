using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            Expression = expression;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundConversionExpression;
        public override TypeSymbol ResultType => Type;
        public TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }
}