using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class GlobalStatementSynatx : MemberSyntax
    {
        public GlobalStatementSynatx(StatementSyntax statement, bool isValid, TextLocation location) : base(isValid, location)
        {
            Statement = statement;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.GlobalStatementSynatx;
        public StatementSyntax Statement { get; }

        public override string ToString() => Statement.ToString();
    }

}