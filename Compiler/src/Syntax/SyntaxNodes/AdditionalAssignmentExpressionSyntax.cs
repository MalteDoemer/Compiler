using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class AdditionalAssignmentExpressionSyntax : ExpressionSyntax
    {
        public AdditionalAssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken op, ExpressionSyntax expression, bool isValid, TextLocation location) : base(isValid, location)
        {
            Identifier = identifier;
            Op = op;
            Expression = expression;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.AdditionalAssignmentExpressionSyntax;
        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return Op;
            yield return Expression;
        }
    }
}