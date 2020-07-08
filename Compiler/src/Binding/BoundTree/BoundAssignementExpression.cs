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
        public override BoundNodeKind Kind => BoundNodeKind.BoundAssignementExpression;
        public override TypeSymbol ResultType => Variable.Type;
        public override bool IsValid { get; set; }
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}