using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement(BoundExpression condition, BoundStatement body, BoundStatement elseStatement)
        {
            Condition = condition;
            Body = body;
            ElseStatement = elseStatement;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public BoundStatement ElseStatement { get; }
    }
}