using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(TypeSymbol? type, BoundExpression expression, bool isValid) : base(isValid)
        {
            ResultType = type is null ? TypeSymbol.ErrorType : type;
            Expression = expression;
            if (isValid && !(type is null))
                Constant = ConstantFolder.FoldConversion(ResultType, expression);
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundConversionExpression;
        public override TypeSymbol ResultType { get; }
        public override BoundConstant? Constant { get; }
        public BoundExpression Expression { get; }
    }
}