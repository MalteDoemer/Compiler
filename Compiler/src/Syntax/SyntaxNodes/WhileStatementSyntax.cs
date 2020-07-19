using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax(SyntaxToken whileToken, ExpressionSyntax condition, StatementSyntax body, bool isValid, TextLocation location) : base(isValid, location)
        {
            WhileToken = whileToken;
            Condition = condition;
            Body = body;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.WhileStatementSyntax;
        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return WhileToken;
            yield return Condition;
            yield return Body;
        }
    }
}