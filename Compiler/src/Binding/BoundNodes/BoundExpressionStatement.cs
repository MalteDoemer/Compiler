using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression, bool isValid, bool shouldPop ) : base(isValid)
        {
            Expression = expression;
            ShouldPop = shouldPop;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundExpressionStatement;
        public BoundExpression Expression { get; }
        public bool ShouldPop { get; }
    }
}