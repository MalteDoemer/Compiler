namespace Compiler.Binding
{
    internal sealed class BoundDoWhileStatement : BoundStatement
    {
        public BoundDoWhileStatement(BoundStatement body, BoundExpression condition, bool isValid)
        {
            Body = body;
            Condition = condition;
            IsValid = isValid;
        }

        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }

        public override bool IsValid { get; }
    }
}