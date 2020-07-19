using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken forToken, StatementSyntax variableDacleration, ExpressionSyntax condition, ExpressionSyntax increment, StatementSyntax body, bool isValid, TextLocation location) : base(isValid, location)
        {
            ForToken = forToken;
            VariableDeclaration = variableDacleration;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ForStatementSyntax;
        public SyntaxToken ForToken { get; }
        public StatementSyntax VariableDeclaration { get; }
        public ExpressionSyntax Condition { get; }
        public ExpressionSyntax Increment { get; }
        public StatementSyntax Body { get; }
    }
}