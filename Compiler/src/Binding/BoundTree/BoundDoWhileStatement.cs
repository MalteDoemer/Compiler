namespace Compiler.Binding
{
    internal sealed class BoundDoWhileStatement : BoundStatement
    {
        public BoundDoWhileStatement(BoundStatement body, BoundExpression condition, BoundLabel breakLabel, BoundLabel continueLabel)
        {
            Body = body;
            Condition = condition;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}