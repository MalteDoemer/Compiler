using Compiler.Text;

namespace Compiler.Binding
{
    internal class BoundInvalidStatement : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.BoundInvalidStatement;
    }
}