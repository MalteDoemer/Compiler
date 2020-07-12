using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class DoWhileStatementSyntax : StatementSyntax
    {
        public DoWhileStatementSyntax(SyntaxToken doToken, StatementSyntax body, SyntaxToken whileToken, ExpressionSyntax condition, bool isValid = true)
        {
            DoToken = doToken;
            Body = body;
            WhileToken = whileToken;
            Condition = condition;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.DoWhileStatementSyntax;
        public override TextSpan Location => DoToken.Span + Condition.Location;
        public override bool IsValid { get; }

        public SyntaxToken DoToken { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }

        public override string ToString() => $"do\n{Body}\n\twhile{Condition}";
    }
}