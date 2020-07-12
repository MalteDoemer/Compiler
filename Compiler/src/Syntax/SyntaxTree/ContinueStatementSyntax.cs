using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax(SyntaxToken continueToken, bool isValid, TextLocation location)
        {
            ContinueToken = continueToken;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ContinueStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken ContinueToken { get; }
        public override string ToString() => ContinueToken.ToString();
    }
}