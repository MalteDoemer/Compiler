using Compiler.Text;

namespace Compiler.Binding
{
    internal class BoundInvalidStatement : BoundStatement
    {
        public BoundInvalidStatement(TextSpan span)
        {
            Span = span;
        }

        public override TextSpan Span { get; }
    }
}