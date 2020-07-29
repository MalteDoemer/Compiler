using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class TernaryExpressionSyntax : ExpressionSyntax
    {
        public TernaryExpressionSyntax(ExpressionSyntax condition, SyntaxToken questionMark, ExpressionSyntax thenExpression, SyntaxToken colonToken, ExpressionSyntax elseExpression, bool isValid, TextLocation location) : base(isValid, location)
        {
            Condition = condition;
            QuestionMark = questionMark;
            ThenExpression = thenExpression;
            ColonToken = colonToken;
            ElseExpression = elseExpression;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.TernaryExpressionSyntax;

        public ExpressionSyntax Condition { get; }
        public SyntaxToken QuestionMark { get; }
        public ExpressionSyntax ThenExpression { get; }
        public SyntaxToken ColonToken { get; }
        public ExpressionSyntax ElseExpression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Condition;
            yield return QuestionMark;
            yield return ThenExpression;
            yield return ColonToken;
            yield return ElseExpression;
        }
    }
}