using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundTernaryExpression : BoundExpression
    {
        public BoundTernaryExpression(BoundExpression condition, BoundExpression thenExpression, BoundExpression elseExpression, TypeSymbol resultType, bool isValid) : base(isValid)
        {
            Condition = condition;
            ThenExpression = thenExpression;
            ElseExpression = elseExpression;
            ResultType = resultType;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundTernaryExpression;
        public override TypeSymbol ResultType { get; }
        public BoundExpression Condition { get; }
        public BoundExpression ThenExpression { get; }
        public BoundExpression ElseExpression { get; }
    }
}