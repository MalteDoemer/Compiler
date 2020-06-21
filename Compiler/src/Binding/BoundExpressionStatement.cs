using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression, TextSpan span)
        {
            Expression = expression;
            Span = span;
        }

        public override TextSpan Span { get; }

        public BoundExpression Expression { get; }
    }
}