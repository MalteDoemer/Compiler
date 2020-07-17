using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {
        public BoundForStatement(BoundStatement variableDeclaration, BoundExpression condition, BoundExpression increment, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel, bool isValid) : base(isValid)
        {
            VariableDeclaration = variableDeclaration;
            Condition = condition;
            Increment = increment;
            Body = body;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundForStatement;
        public BoundStatement VariableDeclaration { get; }
        public BoundExpression Condition { get; }
        public BoundExpression Increment { get; }
        public BoundStatement Body { get; }
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}