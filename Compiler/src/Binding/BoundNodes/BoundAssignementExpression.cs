using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression, bool isValid) : base(isValid) 
        {
            Variable = variable;
            Expression = expression;
        }
        
        public override BoundNodeKind Kind => BoundNodeKind.BoundAssignmentExpression;
        public override TypeSymbol ResultType => Variable.Type;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}