using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class DoWhileStatementSyntax : StatementSyntax
    {
        public DoWhileStatementSyntax(SyntaxToken doToken, StatementSyntax body, SyntaxToken whileToken, ExpressionSyntax condition)
        {
            DoToken = doToken;
            Body = body;
            WhileToken = whileToken;
            Condition = condition;
        }

        public override TextSpan Span => DoToken.Span + Condition.Span;
        public override bool IsValid => DoToken.IsValid && Body.IsValid && WhileToken.IsValid && Condition.IsValid;

        public SyntaxToken DoToken { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }

        public override string ToString() => $"do\n{Body}\n\twhile{Condition}";
    }
}