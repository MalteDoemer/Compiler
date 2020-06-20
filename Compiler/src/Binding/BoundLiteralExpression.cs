namespace Compiler.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        private readonly int pos;
        private readonly TypeSymbol symbol;

        public BoundLiteralExpression(int pos, dynamic value, TypeSymbol symbol)
        {
            this.pos = pos;
            Value = value;
            this.symbol = symbol;
        }

        public override TypeSymbol ResultType => symbol;
        public override int Pos => pos;
        public dynamic Value { get; }
    }
}