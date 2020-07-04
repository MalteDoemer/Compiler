using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ExpressionStatement : StatementSyntax
    {
        public ExpressionStatement(ExpressionSyntax expression, bool isValid = true)
        {
            Expression = expression;
            IsValid = isValid;
        }

        public override TextSpan Span => Expression.Span;

        public ExpressionSyntax Expression { get; }

        public override bool IsValid { get; }

        public override string ToString() => Expression.ToString();
    }
}