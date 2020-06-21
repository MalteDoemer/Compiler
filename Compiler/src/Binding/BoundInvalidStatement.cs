using Compiler.Text;

namespace Compiler.Binding
{
    internal class BoundInvalidStatement : BoundStatement
    {
        public override TextSpan Span => TextSpan.Invalid;
    }
}