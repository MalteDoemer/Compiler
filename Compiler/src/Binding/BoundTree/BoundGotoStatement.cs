namespace Compiler.Binding
{
    internal sealed class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement(BoundLabel label)
        {
            Label = label;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundGotoStatement;
        public BoundLabel Label { get; }
    }

}