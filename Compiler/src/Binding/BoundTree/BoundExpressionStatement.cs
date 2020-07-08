using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression)
        {
            Expression = expression;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundExpressionStatement;
        public BoundExpression Expression { get; }
    }
}