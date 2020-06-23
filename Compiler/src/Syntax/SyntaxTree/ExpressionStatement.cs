using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ExpressionStatement : StatementSyntax
    {
        public ExpressionStatement(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public override TextSpan Span => Expression.Span;

        public ExpressionSyntax Expression { get; }
    }
}