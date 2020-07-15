using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression expression, TypeSymbol resultType, bool isValid)
        {
            ResultType = resultType;
            Op = op;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundUnaryExpression;
        public override TypeSymbol ResultType { get; }
        public override bool IsValid { get; }
        public BoundUnaryOperator Op { get; }
        public BoundExpression Expression { get; }
    }
}