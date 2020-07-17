using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax(SyntaxToken continueToken, bool isValid, TextLocation location) : base(isValid, location)
        {
            ContinueToken = continueToken;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ContinueStatementSyntax;
        public SyntaxToken ContinueToken { get; }
        public override string ToString() => ContinueToken.ToString();
    }
}