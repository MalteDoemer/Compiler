namespace Compiler.Binding
{
    internal sealed class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement(BoundExpression expression, bool isValid)
        {
            Expression = expression;
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundReturnStatement;
        public override bool IsValid { get; }
        public BoundExpression Expression { get; }
    }
}