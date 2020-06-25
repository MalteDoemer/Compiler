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
}