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
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundVariableExpression;
        public override TypeSymbol ResultType => Variable.Type;
        public override bool IsValid { get; set; }
        public VariableSymbol Variable { get; }
    }
}