using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class ElseStatementSyntax : StatementSyntax
    {
        internal ElseStatementSyntax(SyntaxToken elseToken, StatementSyntax thenStatement, bool isValid, TextLocation location) : base(isValid, location)
        {
            ElseToken = elseToken;
            Body = thenStatement;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ElseStatementSyntax;
        public SyntaxToken ElseToken { get; }
        public StatementSyntax Body { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ElseToken;
            yield return Body;
        }
    }
}