using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundAssignementExpression : BoundExpression
    {
        public BoundAssignementExpression(VariableSymbol variable, BoundExpression expression)
        {
            Variable = variable;
            Expression = expression;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundAssignementExpression;
        public override TypeSymbol ResultType => Variable.Type;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}