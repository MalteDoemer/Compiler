using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax(ExpressionSyntax expression, bool isValid = true)
        {
            Expression = expression;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ExpressionStatementSyntax;
        public override TextSpan Span => Expression.Span;
        public override bool IsValid { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => Expression.ToString();
    }
}