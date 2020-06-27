using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ElseStatementSyntax : StatementSyntax
    {
        public ElseStatementSyntax(SyntaxToken elseToken, StatementSyntax thenStatement)
        {
            ElseToken = elseToken;
            Body = thenStatement;
        }

        public override TextSpan Span => ElseToken.Span + Body.Span;

        public SyntaxToken ElseToken { get; }
        public StatementSyntax Body { get; }

        public override bool IsValid => ElseToken.IsValid && Body.IsValid;

        public override string ToString()
        {
            return $"{ElseToken.Value} {Body}";
        }
    }
}