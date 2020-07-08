using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableDeclarationStatement : BoundStatement
    {
        public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression expr, bool isValid)
        {
            Variable = variable;
            Expression = expr;
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundVariableDeclarationStatement;
        public override bool IsValid { get; }
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}