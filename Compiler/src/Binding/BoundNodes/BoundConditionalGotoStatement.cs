namespace Compiler.Binding
{
    internal sealed class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement(BoundLabel label, BoundExpression condition, bool jumpIfFalse, bool isValid) : base(isValid)
        {
            Label = label;
            Condition = condition;
            JumpIfFalse = jumpIfFalse;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundConditionalGotoStatement;
        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfFalse { get; }
    }

}