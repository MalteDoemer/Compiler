namespace Compiler.Binding
{
    internal sealed class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement(BoundLabel label, bool isValid)
        {
            Label = label;
            IsValid = isValid;
        }

        public BoundLabel Label { get; }

        public override bool IsValid { get; }
    }

}