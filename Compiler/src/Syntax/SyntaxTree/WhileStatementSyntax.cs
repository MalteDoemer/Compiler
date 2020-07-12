using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax(SyntaxToken whileToken, ExpressionSyntax condition, StatementSyntax body, bool isValid = true)
        {
            WhileToken = whileToken;
            Condition = condition;
            Body = body;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.WhileStatementSyntax;
        public override TextSpan Location => WhileToken.Span + Body.Location;
        public override bool IsValid { get; }
        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"while {Condition}\n{Body}";
    }
}