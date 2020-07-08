namespace Compiler.Binding
{
    internal sealed class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement(BoundLabel label, bool isValid)
        {
            Label = label;
            IsValid = isValid;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundGotoStatement;
        public override bool IsValid { get; }
        public BoundLabel Label { get; }
    }

}