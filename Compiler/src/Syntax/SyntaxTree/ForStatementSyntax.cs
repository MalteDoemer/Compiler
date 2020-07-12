using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken forToken, StatementSyntax variableDacleration, ExpressionSyntax condition, ExpressionSyntax increment, StatementSyntax body, bool isValid, TextLocation location)
        {
            ForToken = forToken;
            VariableDeclaration = variableDacleration;
            Condition = condition;
            Increment = increment;
            Body = body;
            IsValid = isValid;
            Location = location;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ForStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken ForToken { get; }
        public StatementSyntax VariableDeclaration { get; }
        public ExpressionSyntax Condition { get; }
        public ExpressionSyntax Increment { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"for {VariableDeclaration}, {Condition}, {Increment}\n{Body}";
    }
}