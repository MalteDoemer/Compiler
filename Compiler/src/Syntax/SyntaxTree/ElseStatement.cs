using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ElseStatement : StatementSyntax
    {
        public ElseStatement(SyntaxToken elseToken, StatementSyntax thenStatement)
        {
            ElseToken = elseToken;
            ThenStatement = thenStatement;
        }

        public override TextSpan Span => ElseToken.Span + ThenStatement.Span;

        public SyntaxToken ElseToken { get; }
        public StatementSyntax ThenStatement { get; }

        public override string ToString()
        {
            return $"{ElseToken.Value} {ThenStatement}";
        }
    }
}