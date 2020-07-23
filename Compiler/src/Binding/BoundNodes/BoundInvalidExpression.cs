using Compiler.Symbols;

namespace Compiler.Binding
{
    // TODO remove this
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public BoundInvalidExpression() : base(false)
        {
        }

        public override TypeSymbol ResultType => TypeSymbol.Invalid;
        public override BoundNodeKind Kind => BoundNodeKind.BoundInvalidExpression;
    }
}