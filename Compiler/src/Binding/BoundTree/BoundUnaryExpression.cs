using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression right, TypeSymbol resultType, bool isValid)
        {
            ResultType = resultType;
            Op = op;
            Right = right;
        }


        public BoundUnaryOperator Op { get; }
        public BoundExpression Right { get; }
        public TextSpan OperatorSpan { get; }

        public override TypeSymbol ResultType { get; }

        public override bool IsValid { get; }
    }
}