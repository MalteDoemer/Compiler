using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken equalToken,  ExpressionSyntax expression)
        {
            Identifier = identifier;
            EqualToken = equalToken;
            Expression = expression;
        }

        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }
        public override TextSpan Span => Identifier.Span + Expression.Span;

        public override bool IsValid => Identifier.IsValid && EqualToken.IsValid && Expression.IsValid;

        public override string ToString()
        {
            return $"{Identifier.Value} = {Expression}";
        }
    }
}