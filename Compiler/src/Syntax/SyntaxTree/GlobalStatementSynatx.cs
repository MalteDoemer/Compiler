using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class GlobalStatementSynatx : MemberSyntax
    {
        public GlobalStatementSynatx(StatementSyntax statement, bool isValid = true)
        {
            Statement = statement;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.GlobalStatementSynatx;
        public override TextSpan Location => Statement.Location;
        public override bool IsValid { get; }
        public StatementSyntax Statement { get; }

        public override string ToString() => Statement.ToString();
    }

}