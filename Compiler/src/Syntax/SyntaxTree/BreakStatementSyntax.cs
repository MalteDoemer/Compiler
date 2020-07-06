using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax(SyntaxToken breakToken, bool isValid)
        {
            BreakToken = breakToken;
            IsValid = isValid;
        }

        public override TextSpan Span => BreakToken.Span;
        public override bool IsValid { get; }
        public SyntaxToken BreakToken { get; }
        public override string ToString() => BreakToken.ToString();
    }
}