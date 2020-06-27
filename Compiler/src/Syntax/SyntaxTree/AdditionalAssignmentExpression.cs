using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class AdditionalAssignmentExpression : ExpressionSyntax
    {
        public AdditionalAssignmentExpression(SyntaxToken identifier, SyntaxToken op, ExpressionSyntax expression)
        {
            Identifier = identifier;
            Op = op;
            Expression = expression;
        }

        public override TextSpan Span => Identifier.Span + Expression.Span;
        public override bool IsValid => Identifier.IsValid && Op.IsValid && Expression.IsValid;

        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"({Identifier} {Op.Value} {Expression})";
    }
}