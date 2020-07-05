using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration(VariableSymbol variable, BoundExpression expr, bool isValid)
        {
            Variable = variable;
            Expression = expr;
            IsValid = isValid;
        }


        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }

        public override bool IsValid { get; }
    }
}