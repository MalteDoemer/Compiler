using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken forToken, StatementSyntax variableDecleration, ExpressionSyntax condition, ExpressionSyntax increment, StatementSyntax body)
        {
            ForToken = forToken;
            VariableDecleration = variableDecleration;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public override TextSpan Span => ForToken.Span + Body.Span;
        public override bool IsValid => ForToken.IsValid && VariableDecleration.IsValid && Condition.IsValid && Increment.IsValid && Body.IsValid;

        public SyntaxToken ForToken { get; }
        public StatementSyntax VariableDecleration { get; }
        public ExpressionSyntax Condition { get; }
        public ExpressionSyntax Increment { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"for {VariableDecleration}, {Condition}, {Increment}\n{Body}";
    }
}