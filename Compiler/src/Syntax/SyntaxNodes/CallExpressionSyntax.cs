using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class CallExpressionSyntax : ExpressionSyntax
    {
        internal CallExpressionSyntax(SyntaxToken identifier, SyntaxToken leftParenthesis, SeperatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken rightParenthesis, bool isValid, TextLocation location) : base(isValid, location)
        {
            Identifier = identifier;
            LeftParenthesis = leftParenthesis;
            Arguments = arguments;
            RightParenthesis = rightParenthesis;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.CallExpressionSyntax;
        public SyntaxToken Identifier { get; }
        public SyntaxToken LeftParenthesis { get; }
        public SeperatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken RightParenthesis { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return LeftParenthesis;
            foreach (var arg in Arguments)
                yield return arg;
            yield return RightParenthesis;
        }
    }
}