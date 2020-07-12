using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class AdditionalAssignmentExpressionSyntax : ExpressionSyntax
    {
        public AdditionalAssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken op, ExpressionSyntax expression, bool isValid, TextLocation location)
        {
            Identifier = identifier;
            Op = op;
            Expression = expression;
            IsValid = isValid;
            Location = location;
        }

        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.AdditionalAssignmentExpressionSyntax;

        public override string ToString() => $"({Identifier} {Op.Value} {Expression})";
    }
}