using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variable, bool isValid)
        {
            Variable = variable;
            IsValid = isValid;
            if (isValid)
                Constant = variable.Constant;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundVariableExpression;
        public override TypeSymbol ResultType => Variable.Type;
        public override BoundConstant Constant { get; }
        public override bool IsValid { get; }
        public VariableSymbol Variable { get; }
    }
}