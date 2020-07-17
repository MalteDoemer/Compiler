namespace Compiler.Binding
{
    internal sealed class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement(BoundLabel label, bool isValid) : base(isValid)
        {
            Label = label;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundGotoStatement;
        public BoundLabel Label { get; }
    }

}