namespace Compiler.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly int pos;
        private readonly TypeSymbol resultType;

        public BoundUnaryExpression(int pos, BoundUnaryOperator op, BoundExpression right, TypeSymbol resultType)
        {
            this.resultType = resultType;
            this.pos = pos;
            Op = op;
            Right = right;
        }

        public override TypeSymbol ResultType => resultType;

        public BoundUnaryOperator Op { get; }
        public BoundExpression Right { get; }

        public override int Pos => pos;
    }
}