using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax(ExpressionSyntax expression, bool isValid, TextLocation location)
        {
            Expression = expression;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ExpressionStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => Expression.ToString();
    }
}