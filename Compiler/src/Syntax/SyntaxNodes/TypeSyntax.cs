using Compiler.Text;

namespace Compiler.Syntax
{
    public abstract class TypeSyntax : SyntaxNode
    {
        protected TypeSyntax(bool isValid, TextLocation location) : base(isValid, location)
        {
        }
    }

}