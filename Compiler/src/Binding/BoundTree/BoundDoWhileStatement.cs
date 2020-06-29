namespace Compiler.Binding
{
    internal sealed class BoundDoWhileStatement : BoundStatement
    {
        public BoundDoWhileStatement(BoundStatement body, BoundExpression condition)
        {
            Body = body;
            Condition = condition;
        }

        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
    }
}