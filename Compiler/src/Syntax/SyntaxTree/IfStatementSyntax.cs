using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class IfStatementSyntax : StatementSyntax
    {

        public IfStatementSyntax(SyntaxToken ifToken, ExpressionSyntax condition, StatementSyntax body, ElseStatementSyntax elseStatement, bool isValid = true)
        {
            IfToken = ifToken;
            Condition = condition;
            Body = body;
            ElseStatement = elseStatement;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.IfStatementSyntax;
        public override TextSpan Span => IfToken.Span + (ElseStatement == null ? Body.Span : ElseStatement.Span);
        public override bool IsValid { get; }
        public SyntaxToken IfToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
        public ElseStatementSyntax ElseStatement { get; }

        public override string ToString() => $"{IfToken.Value} ({Condition} {Body} {(ElseStatement == null ? "" : ElseStatement.ToString())})";
    }
}