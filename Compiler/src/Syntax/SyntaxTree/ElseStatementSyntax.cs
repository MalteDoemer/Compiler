using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class ElseStatementSyntax : StatementSyntax
    {
        public ElseStatementSyntax(SyntaxToken elseToken, StatementSyntax thenStatement, bool isValid, TextLocation location) : base(isValid, location)
        {
            ElseToken = elseToken;
            Body = thenStatement;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ElseStatementSyntax;
        public SyntaxToken ElseToken { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"{ElseToken.Value} {Body}";
    }
}