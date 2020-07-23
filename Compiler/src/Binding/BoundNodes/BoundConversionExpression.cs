using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(TypeSymbol type, BoundExpression expression, bool isValid) : base(isValid)
        {
            ResultType = type;
            Expression = expression;
            if (isValid)
                Constant = ConstantFolder.FoldConversion(type, expression);
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundConversionExpression;
        public override TypeSymbol ResultType { get; }
        public override BoundConstant? Constant { get; }
        public BoundExpression Expression { get; }
    }
}