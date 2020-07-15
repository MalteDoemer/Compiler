namespace Compiler.Binding
{
    internal sealed class BoundNopStatement : BoundStatement
    {
        public BoundNopStatement(bool isValid)
        {
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundNopStatement;
        public override bool IsValid { get; }
    }
}