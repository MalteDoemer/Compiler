using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration(VariableSymbol variable, BoundExpression expr)
        {
            Variable = variable;
            Expression = expr;
        }


        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}