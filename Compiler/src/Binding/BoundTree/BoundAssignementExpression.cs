using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundAssignementExpression : BoundExpression
    {
        public BoundAssignementExpression(VariableSymbol variable, BoundExpression expression, TextSpan identifierSpan, TextSpan equalSpan)
        {
            IdentifierSpan = identifierSpan;
            EqualSpan = equalSpan;
            Variable = variable;
            Expression = expression;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
        public TextSpan IdentifierSpan { get; }
        public TextSpan EqualSpan { get; }
        public override TypeSymbol ResultType => Variable.Type;
        public override TextSpan Span => IdentifierSpan + Expression.Span;
    }
}