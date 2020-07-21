using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        internal IfStatementSyntax(SyntaxToken ifToken, ExpressionSyntax condition, StatementSyntax body, ElseStatementSyntax? elseStatement, bool isValid, TextLocation? location) : base(isValid, location)
        {
            IfToken = ifToken;
            Condition = condition;
            Body = body;
            ElseStatement = elseStatement;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.IfStatementSyntax;
        public SyntaxToken IfToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
        public ElseStatementSyntax? ElseStatement { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IfToken;
            yield return Condition;
            yield return Body;
            if (ElseStatement is not null)
                yield return ElseStatement;
        }
    }
}