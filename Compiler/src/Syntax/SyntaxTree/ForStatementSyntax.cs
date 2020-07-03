using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken forToken, StatementSyntax variableDacleration, ExpressionSyntax condition, ExpressionSyntax increment, StatementSyntax body)
        {
            ForToken = forToken;
            VariableDeclaration = variableDacleration;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public override TextSpan Span => ForToken.Span + Body.Span;
        public override bool IsValid => ForToken.IsValid && VariableDeclaration.IsValid && Condition.IsValid && Increment.IsValid && Body.IsValid;

        public SyntaxToken ForToken { get; }
        public StatementSyntax VariableDeclaration { get; }
        public ExpressionSyntax Condition { get; }
        public ExpressionSyntax Increment { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"for {VariableDeclaration}, {Condition}, {Increment}\n{Body}";
    }
}