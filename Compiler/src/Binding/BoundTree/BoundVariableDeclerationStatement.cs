using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableDeclerationStatement : BoundStatement
    {
        public BoundVariableDeclerationStatement(VariableSymbol variable, BoundExpression expr)
        {
            Variable = variable;
            Expression = expr;
        }


        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}