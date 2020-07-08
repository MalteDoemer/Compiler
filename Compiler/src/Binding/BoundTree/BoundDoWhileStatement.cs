namespace Compiler.Binding
{
    internal sealed class BoundDoWhileStatement : BoundStatement
    {
        public BoundDoWhileStatement(BoundStatement body, BoundExpression condition, BoundLabel breakLabel, BoundLabel continueLabel, bool isValid)
        {
            Body = body;
            Condition = condition;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
            IsValid = isValid;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundDoWhileStatement;
        public override bool IsValid { get; }
        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}