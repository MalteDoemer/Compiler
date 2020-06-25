using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class WhileStatement : StatementSyntax
    {
        public WhileStatement(SyntaxToken whileToken, ExpressionSyntax condition, StatementSyntax body)
        {
            WhileToken = whileToken;
            Condition = condition;
            Body = body;
        }

        public override TextSpan Span => WhileToken.Span + Body.Span;
        public override bool IsValid => WhileToken.IsValid && Condition.IsValid && Body.IsValid;

        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }

        public override string ToString() => $"while {Condition}\n{Body}";
    }
}