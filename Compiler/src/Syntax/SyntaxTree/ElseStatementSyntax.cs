using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ElseStatementSyntax : StatementSyntax
    {
        public ElseStatementSyntax(SyntaxToken elseToken, StatementSyntax thenStatement, bool isValid, TextLocation location)
        {
            ElseToken = elseToken;
            Body = thenStatement;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ElseStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken ElseToken { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"{ElseToken.Value} {Body}";
    }
}