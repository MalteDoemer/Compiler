using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class ParenthesizedExpression : ExpressionSyntax
    {
        internal ParenthesizedExpression(SyntaxToken leftParenthesis, ExpressionSyntax expression, SyntaxToken rightParenthesis, bool isValid, TextLocation location) : base(isValid, location)
        {
            LeftParenthesis = leftParenthesis;
            Expression = expression;
            RightParenthesis = rightParenthesis;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ParenthesizedExpression;

        public SyntaxToken LeftParenthesis { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken RightParenthesis { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LeftParenthesis;
            yield return Expression;
            yield return RightParenthesis;
        }
    }
}