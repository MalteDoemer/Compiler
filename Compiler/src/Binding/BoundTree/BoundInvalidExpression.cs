using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.BoundInvalidExpression;
        public override TypeSymbol ResultType => TypeSymbol.ErrorType;
    }
}