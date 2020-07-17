using Compiler.Text;

namespace Compiler.Syntax
{
    internal abstract class MemberSyntax : SyntaxNode
    {
        protected MemberSyntax(bool isValid, TextLocation location) : base(isValid, location)
        {
        }
    }

}