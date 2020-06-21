using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableDeclerationStatement : BoundStatement
    {
        public BoundVariableDeclerationStatement(VariableSymbol variable, BoundExpression expr, TextSpan typeSpan, TextSpan identifierSpan, TextSpan equalSpan, TextSpan epxressionSpan)
        {
            Variable = variable;
            Expression = expr;
            TypeSpan = typeSpan;
            IdentifierSpan = identifierSpan;
            EqualSpan = equalSpan;
            EpxressionSpan = epxressionSpan;
        }


        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
        public TextSpan TypeSpan { get; }
        public TextSpan IdentifierSpan { get; }
        public TextSpan EqualSpan { get; }
        public TextSpan EpxressionSpan { get; }
        public override TextSpan Span => TypeSpan + EpxressionSpan;
    }
}