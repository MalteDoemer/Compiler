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

        public BoundExpression Expression { get; }

        public override bool IsValid { get; }
    }
}