using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken equalToken,  ExpressionSyntax expression, bool isValid, TextLocation location)
        {
            Identifier = identifier;
            EqualToken = equalToken;
            Expression = expression;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.AssignmentExpressionSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"{Identifier.Value} = {Expression}";
    }
}