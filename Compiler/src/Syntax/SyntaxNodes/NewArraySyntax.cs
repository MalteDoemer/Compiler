using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class NewArraySyntax : ExpressionSyntax
    {
        public NewArraySyntax(SyntaxToken newToken, ArrayTypeSyntax arrayTypeSyntax, bool isValid, TextLocation location) : base(isValid, location)
        {
            NewToken = newToken;
            ArrayTypeSyntax = arrayTypeSyntax;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.NewArraySyntax;

        public SyntaxToken NewToken { get; }
        public ArrayTypeSyntax ArrayTypeSyntax { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NewToken;
            yield return ArrayTypeSyntax;
        }
    }
}