using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body, TextSpan whileTokenSpan)
        {
            Condition = condition;
            Body = body;
            WhileTokenSpan = whileTokenSpan;
        }

        public override TextSpan Span => WhileTokenSpan + Body.Span;

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public TextSpan WhileTokenSpan { get; }
    }
}