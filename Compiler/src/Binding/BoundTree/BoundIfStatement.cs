using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement(BoundExpression condition, BoundStatement body, BoundStatement elseStatement, bool isValid)
        {
            Condition = condition;
            Body = body;
            ElseStatement = elseStatement;
            IsValid = isValid;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundIfStatement;
        public override bool IsValid { get; set; }
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public BoundStatement ElseStatement { get; }
    }
}