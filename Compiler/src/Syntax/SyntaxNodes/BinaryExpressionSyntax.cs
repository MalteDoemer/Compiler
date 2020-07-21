using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        internal BinaryExpressionSyntax(SyntaxToken op, ExpressionSyntax left, ExpressionSyntax right, bool isValid, TextLocation location) : base(isValid, location)
        {
            Op = op;
            Left = left;
            Right = right;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.BinaryExpressionSyntax;
        public ExpressionSyntax Left { get; }
        public SyntaxToken Op { get; }
        public ExpressionSyntax Right { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return Op;
            yield return Right;
        }
    }
}