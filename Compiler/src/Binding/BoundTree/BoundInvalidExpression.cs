using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public override TypeSymbol ResultType => TypeSymbol.ErrorType;
    }
}