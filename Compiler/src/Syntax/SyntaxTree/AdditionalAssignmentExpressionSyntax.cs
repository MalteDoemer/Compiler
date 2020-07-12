using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class AdditionalAssignmentExpressionSyntax : ExpressionSyntax
    {
        public AdditionalAssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken op, ExpressionSyntax expression, bool isValid = true)
        {
            Identifier = identifier;
            Op = op;
            Expression = expression;
            IsValid = isValid;
        }

        public override TextSpan Span => Identifier.Span + Expression.Span;
        public override bool IsValid { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.AdditionalAssignmentExpressionSyntax;

        public override string ToString() => $"({Identifier} {Op.Value} {Expression})";
    }
}