using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ElseStatementSyntax : StatementSyntax
    {
        public ElseStatementSyntax(SyntaxToken elseToken, StatementSyntax thenStatement, bool isValid = true)
        {
            ElseToken = elseToken;
            Body = thenStatement;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ElseStatementSyntax;
        public override TextSpan Span => ElseToken.Span + Body.Span;
        public override bool IsValid { get; }
        public SyntaxToken ElseToken { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"{ElseToken.Value} {Body}";
    }
}