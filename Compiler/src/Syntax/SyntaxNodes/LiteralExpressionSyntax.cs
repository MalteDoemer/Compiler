using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        internal LiteralExpressionSyntax(SyntaxToken literal, bool isValid, TextLocation? location) : base(isValid, location)
        {
            Literal = literal;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.LiteralExpressionSyntax;
        public SyntaxToken Literal { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Literal;
        }
    }
}