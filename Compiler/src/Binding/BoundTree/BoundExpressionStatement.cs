using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression, bool isValid)
        {
            Expression = expression;
            IsValid = isValid;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundExpressionStatement;
        public override bool IsValid { get; }
        public BoundExpression Expression { get; }
    }
}