using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class GlobalStatementSynatx : MemberSyntax
    {
        internal GlobalStatementSynatx(StatementSyntax statement, bool isValid, TextLocation? location) : base(isValid, location)
        {
            Statement = statement;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.GlobalStatementSynatx;
        public StatementSyntax Statement { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Statement;
        }
    }
}