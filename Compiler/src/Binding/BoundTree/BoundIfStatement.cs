using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement(BoundExpression condition, BoundStatement body, BoundStatement elseStatement, bool isValid) : base(isValid)
        {
            Condition = condition;
            Body = body;
            ElseStatement = elseStatement;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundIfStatement;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public BoundStatement ElseStatement { get; }
    }
}