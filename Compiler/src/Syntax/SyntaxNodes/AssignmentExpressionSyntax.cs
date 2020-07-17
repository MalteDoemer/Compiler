using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken equalToken, ExpressionSyntax expression, bool isValid, TextLocation location) : base(isValid, location)
        {
            Identifier = identifier;
            EqualToken = equalToken;
            Expression = expression;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.AssignmentExpressionSyntax;
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"{Identifier.Value} = {Expression}";
    }
}