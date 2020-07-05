using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundAssignementExpression : BoundExpression
    {
        public BoundAssignementExpression(VariableSymbol variable, BoundExpression expression, bool isValid)
        {
            Variable = variable;
            Expression = expression;
            IsValid = isValid;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
        public override TypeSymbol ResultType => Variable.Type;

        public override bool IsValid { get; }
    }
}