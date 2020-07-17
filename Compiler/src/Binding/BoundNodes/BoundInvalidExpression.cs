using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public BoundInvalidExpression() : base(false)
        {
        }

        public override TypeSymbol ResultType => TypeSymbol.ErrorType;
        public override BoundNodeKind Kind => BoundNodeKind.BoundInvalidExpression;
    }
}