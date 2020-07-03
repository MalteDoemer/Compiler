using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {
        public BoundForStatement(BoundStatement variableDeclaration, BoundExpression condition, BoundExpression increment, BoundStatement body)
        {
            VariableDeclaration = variableDeclaration;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public BoundStatement VariableDeclaration { get; }
        public BoundExpression Condition { get; }
        public BoundExpression Increment { get; }
        public BoundStatement Body { get; }
    }
}