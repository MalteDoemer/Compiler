using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        internal AssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken equalToken, ExpressionSyntax expression, bool isValid, TextLocation location) : base(isValid, location)
        {
            Identifier = identifier;
            EqualToken = equalToken;
            Expression = expression;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.AssignmentExpressionSyntax;
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return EqualToken;
            yield return Expression;
        }
    }
}