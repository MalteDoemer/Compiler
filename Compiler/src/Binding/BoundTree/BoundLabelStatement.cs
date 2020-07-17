namespace Compiler.Binding
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement(BoundLabel label, bool isValid) : base(isValid)
        {
            Label = label;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundLabelStatement;
        public BoundLabel Label { get; }
    }
}