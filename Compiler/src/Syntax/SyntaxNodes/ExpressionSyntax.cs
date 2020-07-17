using Compiler.Text;

namespace Compiler.Syntax
{
    internal abstract class ExpressionSyntax : SyntaxNode
    {
        protected ExpressionSyntax(bool isValid, TextLocation location) : base(isValid, location)
        {
        }
    }
}