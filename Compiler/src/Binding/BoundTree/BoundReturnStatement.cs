namespace Compiler.Binding
{
    internal sealed class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement(BoundExpression expression, bool isValid) : base(isValid)
        {
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundReturnStatement;
        public BoundExpression Expression { get; }
    }
}