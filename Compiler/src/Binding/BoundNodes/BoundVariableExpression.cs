using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol? variable, bool isValid) : base(isValid)
        {
            Variable = variable;
            if (isValid && !(variable is null))
            {
                Constant = variable.Constant;
                ResultType = variable.Type;
            }
            else 
                ResultType = TypeSymbol.ErrorType;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundVariableExpression;
        public override TypeSymbol ResultType { get; }
        public override BoundConstant? Constant { get; }
        public VariableSymbol? Variable { get; }
    }
}