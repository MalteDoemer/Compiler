namespace Compiler.Binding
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement(BoundLabel label, bool isValid)
        {
            Label = label;
            IsValid = isValid;
        }

        public BoundLabel Label { get; }

        public override bool IsValid { get; }
    }

}