using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly TypeSymbol resultType;

        public BoundUnaryExpression(BoundUnaryOperator op, TextSpan operatorSpan, BoundExpression right, TypeSymbol resultType)
        {
            this.resultType = resultType;
            Op = op;
            OperatorSpan = operatorSpan;
            Right = right;
        }


        public BoundUnaryOperator Op { get; }
        public BoundExpression Right { get; }
        public TextSpan OperatorSpan { get; }

        public override TypeSymbol ResultType => resultType;
        public override TextSpan Span => OperatorSpan + Right.Span;
    }
}