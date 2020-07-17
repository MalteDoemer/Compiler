using Compiler.Symbols;

namespace Compiler.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        protected BoundExpression(bool isValid) : base(isValid)
        {
        }

        public abstract TypeSymbol ResultType { get; }
    }
}