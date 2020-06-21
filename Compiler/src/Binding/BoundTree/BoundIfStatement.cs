using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement(BoundExpression condition, BoundStatement thenStatement, BoundStatement elseStatement, TextSpan ifTokenSpan)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
            IfTokenSpan = ifTokenSpan;
        }

        public override TextSpan Span => IfTokenSpan + (ElseStatement == null ? ThenStatement.Span : ElseStatement.Span);

        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement ElseStatement { get; }
        public TextSpan IfTokenSpan { get; }
    }
}