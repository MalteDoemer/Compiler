using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class GlobalStatementSynatx : MemberSyntax
    {
        public GlobalStatementSynatx(StatementSyntax statement, bool isValid, TextLocation location)
        {
            Statement = statement;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.GlobalStatementSynatx;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public StatementSyntax Statement { get; }

        public override string ToString() => Statement.ToString();
    }

}