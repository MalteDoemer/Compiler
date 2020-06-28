using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableDecleration : BoundStatement
    {
        public BoundVariableDecleration(VariableSymbol variable, BoundExpression expr)
        {
            Variable = variable;
            Expression = expr;
        }


        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}