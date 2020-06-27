using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundPrintStatement : BoundStatement
    {
        public BoundPrintStatement(BoundExpression expression, TextSpan printTokenSpan)
        {
            Expression = expression;
            PrintTokenSpan = printTokenSpan;
        }

        public BoundExpression Expression { get; }
        public TextSpan PrintTokenSpan { get; }
        
        public override TextSpan Span => PrintTokenSpan + Expression.Span;
    }

    
}