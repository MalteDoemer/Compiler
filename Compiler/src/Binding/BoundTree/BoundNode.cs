using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal abstract class BoundNode
    {

    }

    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            Expression = expression;
        }

        public override TypeSymbol ResultType => Type;

        public TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }
}