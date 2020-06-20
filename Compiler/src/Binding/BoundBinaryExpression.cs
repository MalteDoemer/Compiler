using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(BoundBinaryOperator op, TextSpan operatorSpan, BoundExpression left, BoundExpression right, TypeSymbol resultType)
        {
            ResultType = resultType;
            OperatorSpan = operatorSpan;
            Op = op;
            Left = left;
            Right = right;
        }


        public BoundBinaryOperator Op { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }

        public TextSpan OperatorSpan { get; }

        public override TypeSymbol ResultType { get; }
        public override TextSpan Span => Left.Span + Right.Span;
    }
}