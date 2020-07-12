using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken equalToken,  ExpressionSyntax expression, bool isValid = true)
        {
            Identifier = identifier;
            EqualToken = equalToken;
            Expression = expression;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.AssignmentExpressionSyntax;
        public override TextSpan Span => Identifier.Span + Expression.Span;
        public override bool IsValid { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"{Identifier.Value} = {Expression}";
    }
}