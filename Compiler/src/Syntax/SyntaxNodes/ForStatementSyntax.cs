using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        internal ForStatementSyntax(SyntaxToken forToken, StatementSyntax variableDacleration, ExpressionSyntax condition, ExpressionSyntax increment, StatementSyntax body, bool isValid, TextLocation? location) : base(isValid, location)
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

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ForToken;
            yield return VariableDeclaration;
            yield return Condition;
            yield return Increment;
            yield return Body;
        }
    }
}