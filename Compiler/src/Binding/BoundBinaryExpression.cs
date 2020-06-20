namespace Compiler.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        private readonly int pos;
        private readonly TypeSymbol resultType;

        public BoundBinaryExpression(int pos, BoundBinaryOperator op, BoundExpression left, BoundExpression right, TypeSymbol resultType)
        {
            this.pos = pos;
            Op = op;
            Left = left;
            Right = right;
            this.resultType = resultType;
        }

        public override TypeSymbol ResultType => resultType;

        public BoundBinaryOperator Op { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }

        public override int Pos => pos;
    }
}