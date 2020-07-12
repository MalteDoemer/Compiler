using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax(SyntaxToken returnToken, ExpressionSyntax returnExpression, SyntaxToken voidToken, bool isValid)
        {
            IsValid = isValid;
            ReturnToken = returnToken;
            ReturnExpression = returnExpression;
            VoidToken = voidToken;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ReturnStatementSyntax;
        public override TextSpan Span => ReturnToken.Span + (ReturnExpression == null ? VoidToken.Span : ReturnExpression.Span);
        public override bool IsValid { get; }
        public SyntaxToken ReturnToken { get; }
        public ExpressionSyntax ReturnExpression { get; }
        public SyntaxToken VoidToken { get; }

        public override string ToString() => $"return {(ReturnExpression == null ? " void" : ReturnExpression.ToString())}";
    }
}