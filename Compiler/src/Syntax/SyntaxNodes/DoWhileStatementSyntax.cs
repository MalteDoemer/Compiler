using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class DoWhileStatementSyntax : StatementSyntax
    {
        internal DoWhileStatementSyntax(SyntaxToken doToken, StatementSyntax body, SyntaxToken whileToken, ExpressionSyntax condition, bool isValid, TextLocation location) : base(isValid, location)
        {
            DoToken = doToken;
            Body = body;
            WhileToken = whileToken;
            Condition = condition;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.DoWhileStatementSyntax;
        public SyntaxToken DoToken { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return DoToken;
            yield return Body;
            yield return WhileToken;
            yield return Condition;
        }
    }
}