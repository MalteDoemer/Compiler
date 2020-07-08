using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public override TypeSymbol ResultType => TypeSymbol.ErrorType;
        public override BoundNodeKind Kind => BoundNodeKind.BoundInvalidExpression;
        public override bool IsValid => false;
    }
}