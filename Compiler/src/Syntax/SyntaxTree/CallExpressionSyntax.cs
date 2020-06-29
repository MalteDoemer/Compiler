using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class CallExpressionSyntax : ExpressionSyntax
    {
        public CallExpressionSyntax(SyntaxToken identifier, ArgumentList arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }

        public override TextSpan Span => Identifier.Span + Arguments.RightParenthesis.Span;
        public override bool IsValid => Identifier.IsValid && Arguments.IsValid;

        public SyntaxToken Identifier { get; }
        public ArgumentList Arguments { get; }

        public override string ToString() => Identifier.Value.ToString() + Arguments.ToString();
    }
}