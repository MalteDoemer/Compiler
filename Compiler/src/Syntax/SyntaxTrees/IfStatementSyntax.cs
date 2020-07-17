using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax(SyntaxToken ifToken, ExpressionSyntax condition, StatementSyntax body, ElseStatementSyntax elseStatement, bool isValid, TextLocation location) : base(isValid, location)
        {
            IfToken = ifToken;
            Condition = condition;
            Body = body;
            ElseStatement = elseStatement;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.IfStatementSyntax;
        public SyntaxToken IfToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
        public ElseStatementSyntax ElseStatement { get; }

        public override string ToString() => $"{IfToken.Value} ({Condition} {Body} {(ElseStatement == null ? "" : ElseStatement.ToString())})";
    }
}