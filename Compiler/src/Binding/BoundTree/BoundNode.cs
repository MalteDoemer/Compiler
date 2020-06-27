using Compiler.Text;

namespace Compiler.Binding
{
    internal abstract class BoundNode
    {
        public abstract TextSpan Span { get; }
    }

    // internal sealed class BoundAdditionalAddignmentExpression : BoundExpression
    // {
    //     public BoundAdditionalAddignmentExpression(VariableSymbol variable, BoundBinaryOperator op, BoundExpression expression, TypeSymbol resultType, TextSpan identifierSpan, TextSpan equalSpan)
    //     {
    //         Variable = variable;
    //         Op = op;
    //         Expression = expression;
    //         ResultType = resultType;
    //         IdentifierSpan = identifierSpan;
    //         EqualSpan = equalSpan;
    //     }

    //     public override TypeSymbol ResultType { get; }
    //     public override TextSpan Span => IdentifierSpan + Expression.Span;

    //     public TextSpan IdentifierSpan { get; }
    //     public TextSpan EqualSpan { get; }
    //     public VariableSymbol Variable { get; }
    //     public BoundBinaryOperator Op { get; }
    //     public BoundExpression Expression { get; }
    // }
}