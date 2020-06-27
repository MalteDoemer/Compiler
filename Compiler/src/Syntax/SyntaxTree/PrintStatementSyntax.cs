using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class PrintStatementSyntax : StatementSyntax
    {
        public PrintStatementSyntax(SyntaxToken printToken, ExpressionSyntax expression)
        {
            PrintToken = printToken;
            Expression = expression;
        }

        public override TextSpan Span => PrintToken.Span + Expression.Span;

        public override bool IsValid => PrintToken.IsValid && Expression.IsValid;

        public SyntaxToken PrintToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"print {Expression}";
    }
}