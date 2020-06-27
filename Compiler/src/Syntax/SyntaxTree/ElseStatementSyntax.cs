using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ElseStatementSyntax : StatementSyntax
    {
        public ElseStatementSyntax(SyntaxToken elseToken, StatementSyntax thenStatement)
        {
            ElseToken = elseToken;
            ThenStatement = thenStatement;
        }

        public override TextSpan Span => ElseToken.Span + ThenStatement.Span;

        public SyntaxToken ElseToken { get; }
        public StatementSyntax ThenStatement { get; }

        public override bool IsValid => ElseToken.IsValid && ThenStatement.IsValid;

        public override string ToString()
        {
            return $"{ElseToken.Value} {ThenStatement}";
        }
    }
}