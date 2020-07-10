using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression, bool isValid)
        {
            Variable = variable;
            Expression = expression;
            IsValid = isValid;
        }
        
        public override BoundNodeKind Kind => BoundNodeKind.BoundAssignmentExpression;
        public override TypeSymbol ResultType => Variable.Type;
        public override bool IsValid { get; }
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}