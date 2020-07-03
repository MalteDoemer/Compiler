using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class GlobalStatementSynatx : MemberSyntax
    {
        public GlobalStatementSynatx(StatementSyntax statement)
        {
            Statement = statement;
        }

        public override TextSpan Span => Statement.Span;
        public override bool IsValid => Statement.IsValid;
        public StatementSyntax Statement { get; }

        public override string ToString() => Statement.ToString();
    }

}