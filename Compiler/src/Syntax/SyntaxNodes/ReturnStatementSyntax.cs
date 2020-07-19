using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax(SyntaxToken returnToken, ExpressionSyntax returnExpression, SyntaxToken voidToken, bool isValid, TextLocation location) : base(isValid, location)
        {
            ReturnToken = returnToken;
            ReturnExpression = returnExpression;
            VoidToken = voidToken;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ReturnStatementSyntax;
        public SyntaxToken ReturnToken { get; }
        public ExpressionSyntax ReturnExpression { get; }
        public SyntaxToken VoidToken { get; }
    }
}