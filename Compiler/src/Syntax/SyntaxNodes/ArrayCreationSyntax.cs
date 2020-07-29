using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    public sealed class ArrayCreationSyntax : ExpressionSyntax
    {
        public ArrayCreationSyntax(SyntaxToken newToken, ArrayTypeSyntax arrayTypeSyntax, bool isValid, TextLocation location) : base(isValid, location)
        {
            NewToken = newToken;
            ArrayTypeSyntax = arrayTypeSyntax;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.ArrayCreationSyntax;

        public SyntaxToken NewToken { get; }
        public ArrayTypeSyntax ArrayTypeSyntax { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NewToken;
            yield return ArrayTypeSyntax;
        }
    }
}