namespace Compiler.Binding
{
    internal sealed class BoundNopStatement : BoundStatement
    {
        public BoundNopStatement(bool isValid) : base(isValid)
        {
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundNopStatement;
    }
}