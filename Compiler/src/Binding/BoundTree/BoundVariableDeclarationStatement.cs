using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableDeclarationStatement : BoundStatement
    {
        public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression expr, bool isValid) : base(isValid)
        {
            Variable = variable;
            Expression = expr;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundVariableDeclarationStatement;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }
}