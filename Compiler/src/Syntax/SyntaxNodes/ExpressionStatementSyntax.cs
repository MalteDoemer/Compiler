using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax(ExpressionSyntax expression, bool isValid, TextLocation location) : base(isValid, location)
        {
            Expression = expression;
        }
        
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ExpressionStatementSyntax;
        public ExpressionSyntax Expression { get; }

        public override string ToString() => Expression.ToString();
    }
}