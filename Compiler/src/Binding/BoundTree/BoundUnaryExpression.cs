using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperator op, TextSpan operatorSpan, BoundExpression right, TypeSymbol resultType)
        {
            ResultType = resultType;
            Op = op;
            OperatorSpan = operatorSpan;
            Right = right;
        }


        public BoundUnaryOperator Op { get; }
        public BoundExpression Right { get; }
        public TextSpan OperatorSpan { get; }

        public override TypeSymbol ResultType { get; }
        public override TextSpan Span => OperatorSpan + Right.Span;
    }
}