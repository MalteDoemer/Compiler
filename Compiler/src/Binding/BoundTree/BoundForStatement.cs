using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {
        public BoundForStatement(BoundStatement variableDeclaration, BoundExpression condition, BoundExpression increment, BoundStatement body, bool isValid)
        {
            VariableDeclaration = variableDeclaration;
            Condition = condition;
            Increment = increment;
            Body = body;
            IsValid = isValid;
        }

        public BoundStatement VariableDeclaration { get; }
        public BoundExpression Condition { get; }
        public BoundExpression Increment { get; }
        public BoundStatement Body { get; }

        public override bool IsValid { get; }
    }
}