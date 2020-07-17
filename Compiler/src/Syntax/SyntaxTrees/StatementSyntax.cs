using Compiler.Text;

namespace Compiler.Syntax
{
    internal abstract class StatementSyntax : SyntaxNode
    {
        protected StatementSyntax(bool isValid, TextLocation location) : base(isValid, location)
        {
        }
    }
}