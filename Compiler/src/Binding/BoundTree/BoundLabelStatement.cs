namespace Compiler.Binding
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement(BoundLabel label, bool isValid)
        {
            Label = label;
            IsValid = isValid;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundLabelStatement;
        public override bool IsValid { get; set; }
        public BoundLabel Label { get; }
    }

}