using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class CallExpressionSyntax : ExpressionSyntax
    {
        public CallExpressionSyntax(SyntaxToken identifier, SyntaxToken leftParenthesis, SeperatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken rightParenthesis, bool isValid = true)
        {
            Identifier = identifier;
            LeftParenthesis = leftParenthesis;
            Arguments = arguments;
            RightParenthesis = rightParenthesis;
            IsValid = isValid;
        }

        public override TextSpan Span => Identifier.Span + RightParenthesis.Span;
        public override bool IsValid { get; }

        public SyntaxToken Identifier { get; }
        public SyntaxToken LeftParenthesis { get; }
        public SyntaxToken RightParenthesis { get; }
        public SeperatedSyntaxList<ExpressionSyntax> Arguments { get; }

        public override string ToString() => Identifier.Value.ToString() + "(" + Arguments.ToString() + ")";
    }
}