using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class IfStatementSyntax : StatementSyntax
    {

        public IfStatementSyntax(SyntaxToken ifToken, ExpressionSyntax condition, StatementSyntax body, ElseStatementSyntax elseStatement = null)
        {
            IfToken = ifToken;
            Condition = condition;
            Body = body;
            ElseStatement = elseStatement;
        }

        public override TextSpan Span => IfToken.Span + (ElseStatement == null ? Body.Span : ElseStatement.Span);
        public SyntaxToken IfToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
        public ElseStatementSyntax ElseStatement { get; }

        public override bool IsValid => IfToken.IsValid && Condition.IsValid && Body.IsValid && (ElseStatement == null ? true : ElseStatement.IsValid);
        public override string ToString() => $"{IfToken.Value} ({Condition} {Body} {(ElseStatement == null ? "" : ElseStatement.ToString())})";
    }
}