using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundArrayCreation : BoundExpression
    {
        public BoundArrayCreation(TypeSymbol resultType, BoundExpression size, bool isValid) : base(isValid)
        {
            ResultType = resultType;
            UnderlyingType = ((ArrayTypeSymbol)resultType).UnderlyingType;
            Size = size;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundArrayCreation;
        public override TypeSymbol ResultType { get; }
        public TypeSymbol UnderlyingType { get; }
        public BoundExpression Size { get; }
    }
}