using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundPrintStatement : BoundStatement
    {
        public BoundPrintStatement(BoundExpression expression)
        {
            Expression = expression;
        }

        public BoundExpression Expression { get; }
    }
}