using Compiler.Text;

namespace Compiler.Syntax
{
    public abstract class StatementSyntax : SyntaxNode
    {
        protected StatementSyntax(bool isValid, TextLocation location) : base(isValid, location)
        {
        }
    }
}