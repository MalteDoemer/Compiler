using Compiler.Text;

namespace Compiler.Syntax
{
    internal class InvalidStatementSyntax : StatementSyntax
    {
        public InvalidStatementSyntax(TextSpan span)
        {
            Span = span;
        }

        public override TextSpan Span { get; }

        public override bool IsValid => false;

        public override string ToString()
        {
            return "Invalid";
        }
    
        
    }
}