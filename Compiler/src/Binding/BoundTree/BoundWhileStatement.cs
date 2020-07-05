using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body, bool isValid)
        {
            Condition = condition;
            Body = body;
            IsValid = isValid;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }

        public override bool IsValid { get; }
    }
}