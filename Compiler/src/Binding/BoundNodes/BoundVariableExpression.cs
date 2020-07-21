using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variable, bool isValid) : base(isValid)
        {
            Variable = variable;
            if (isValid && variable != null)
                Constant = variable.Constant;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundVariableExpression;
        public override TypeSymbol ResultType => Variable.Type;
        public override BoundConstant Constant { get; }
        public VariableSymbol Variable { get; }
    }
}