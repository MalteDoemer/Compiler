using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class IfStatement : StatementSyntax
    {

        public IfStatement(SyntaxToken ifToken, ExpressionSyntax expression, StatementSyntax thenStatement, ElseStatement elseStatement = null)
        {
            IfToken = ifToken;
            Expression = expression;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }

        public override TextSpan Span => IfToken.Span + (ElseStatement == null ? ThenStatement.Span : ElseStatement.Span);
        public SyntaxToken IfToken { get; }
        public ExpressionSyntax Expression { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseStatement ElseStatement { get; }

        public override string ToString()
        {
            return $"{IfToken.Value} ({Expression} {ThenStatement} {(ElseStatement == null ? "": ElseStatement.ToString())})";
        }
    }
}