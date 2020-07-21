using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        internal ReturnStatementSyntax(SyntaxToken returnToken, ExpressionSyntax returnExpression, SyntaxToken voidToken, bool isValid, TextLocation location) : base(isValid, location)
        {
            ReturnToken = returnToken;
            ReturnExpression = returnExpression;
            VoidToken = voidToken;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ReturnStatementSyntax;
        public SyntaxToken ReturnToken { get; }
        public ExpressionSyntax ReturnExpression { get; }
        public SyntaxToken VoidToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ReturnToken;
            if (ReturnExpression != null)
                yield return ReturnExpression;
            else
                yield return VoidToken;
        }
    }
}