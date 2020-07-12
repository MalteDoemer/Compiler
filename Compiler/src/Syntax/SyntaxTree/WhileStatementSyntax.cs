using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax(SyntaxToken whileToken, ExpressionSyntax condition, StatementSyntax body, bool isValid, TextLocation location)
        {
            WhileToken = whileToken;
            Condition = condition;
            Body = body;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.WhileStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"while {Condition}\n{Body}";
    }
}