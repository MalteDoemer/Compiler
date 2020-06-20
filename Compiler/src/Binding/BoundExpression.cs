namespace Compiler.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol ResultType { get; }
    }
}