using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class PostIncDecExpressionSyntax : ExpressionSyntax
    {
        internal PostIncDecExpressionSyntax(SyntaxToken identifier, SyntaxToken op, bool isValid, TextLocation location) : base(isValid, location)
        {
            Identifier = identifier;
            Op = op;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.PostIncDecExpressionSyntax;
        public SyntaxToken Identifier { get; }
        public SyntaxToken Op { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return Op;
        }
    }
}