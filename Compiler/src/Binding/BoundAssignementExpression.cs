using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundAssignementExpression : BoundExpression
    {
        public BoundAssignementExpression(string identifier,TextSpan identifierSpan, TextSpan equalSpan, BoundExpression expression, TypeSymbol resultType)
        {
            ResultType = resultType;
            Identifier = identifier;
            IdentifierSpan = identifierSpan;
            EqualSpan = equalSpan;
            Expression = expression;
        }

        public string Identifier { get; }
        public TextSpan IdentifierSpan { get; }
        public TextSpan EqualSpan { get; }
        public BoundExpression Expression { get; }
        public override TypeSymbol ResultType { get; }
        public override TextSpan Span => IdentifierSpan + Expression.Span;
    }
}