using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax(SyntaxToken breakToken, bool isValid, TextLocation location)
        {
            BreakToken = breakToken;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.BreakStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken BreakToken { get; }
        
        public override string ToString() => BreakToken.ToString();
    }
}