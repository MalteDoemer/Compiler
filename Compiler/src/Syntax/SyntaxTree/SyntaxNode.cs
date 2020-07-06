using Compiler.Text;



namespace Compiler.Syntax
{

    internal abstract class SyntaxNode
    {
        public abstract TextSpan Span { get; }
        public abstract bool IsValid { get; }

        public abstract override string ToString();

    }

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