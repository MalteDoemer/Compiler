namespace Compiler.Binding
{
    internal sealed class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement(BoundLabel label, BoundExpression condition, bool jumpIfFalse, bool isValid)
        {
            Label = label;
            Condition = condition;
            JumpIfFalse = jumpIfFalse;
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundConditionalGotoStatement;
        public override bool IsValid { get; }
        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfFalse { get; }
    }

}