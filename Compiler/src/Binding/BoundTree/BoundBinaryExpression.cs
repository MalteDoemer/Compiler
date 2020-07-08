using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(BoundBinaryOperator op, BoundExpression left, BoundExpression right, TypeSymbol resultType)
        {
            ResultType = resultType;
            Op = op;
            Left = left;
            Right = right;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundBinaryExpression;
        public BoundBinaryOperator Op { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }

        public override TypeSymbol ResultType { get; }
    }
}