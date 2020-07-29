using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundNewArray : BoundExpression
    {
        public BoundNewArray(TypeSymbol resultType, BoundExpression size, bool isValid) : base(isValid)
        {
            ResultType = resultType;
            UnderlyingType = ((ArrayTypeSymbol)resultType).UnderlyingType;
            Size = size;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundNewArray;
        public override TypeSymbol ResultType { get; }
        public TypeSymbol UnderlyingType { get; }
        public BoundExpression Size { get; }
    }
}