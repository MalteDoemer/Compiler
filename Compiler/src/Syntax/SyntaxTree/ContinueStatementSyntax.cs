using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax(SyntaxToken continueToken, bool isValid)
        {
            ContinueToken = continueToken;
            IsValid = isValid;
        }

        public override TextSpan Span => ContinueToken.Span;
        public override bool IsValid { get; }
        public SyntaxToken ContinueToken { get; }
        public override string ToString() => ContinueToken.ToString();
    }
}