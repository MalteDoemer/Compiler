namespace Compiler.Binding
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement(BoundLabel label)
        {
            Label = label;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundLabelStatement;
        public BoundLabel Label { get; }
    }

}