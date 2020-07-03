using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(TextSpan span, StatementSyntax statement)
        {
            Span = span;
            Statement = statement;
        }

        public override TextSpan Span { get; }
        public StatementSyntax Statement { get; }

        public override bool IsValid => Statement.IsValid;

        public override string ToString()
        {
            return Statement.ToString();
        }
    }

    internal abstract class MemberSyntax : SyntaxNode
    {
    }

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

    internal sealed class FunctionDeclaration
    {
        
    }

}