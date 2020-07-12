using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax(SyntaxToken returnToken, ExpressionSyntax returnExpression, SyntaxToken voidToken, bool isValid, TextLocation location)
        {
            IsValid = isValid;
            Location = location;
            ReturnToken = returnToken;
            ReturnExpression = returnExpression;
            VoidToken = voidToken;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ReturnStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken ReturnToken { get; }
        public ExpressionSyntax ReturnExpression { get; }
        public SyntaxToken VoidToken { get; }

        public override string ToString() => $"return {(ReturnExpression == null ? " void" : ReturnExpression.ToString())}";
    }
}