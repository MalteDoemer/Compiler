using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {
        public BoundForStatement(BoundStatement variableDecleration, BoundExpression condition, BoundExpression increment, BoundStatement body)
        {
            VariableDecleration = variableDecleration;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public BoundStatement VariableDecleration { get; }
        public BoundExpression Condition { get; }
        public BoundExpression Increment { get; }
        public BoundStatement Body { get; }
    }
}