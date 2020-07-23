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
            ResultType = variable.Type;
        }
        
        public override BoundNodeKind Kind => BoundNodeKind.BoundAssignmentExpression;
        public override TypeSymbol ResultType { get; }
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}