using Compiler.Text;

namespace Compiler.Syntax
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        protected ExpressionSyntax(bool isValid, TextLocation location) : base(isValid, location)
        {
        }
    }
}