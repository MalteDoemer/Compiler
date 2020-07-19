using Compiler.Text;

namespace Compiler.Syntax
{
    internal class SyntaxToken : SyntaxNode
    {
        public SyntaxTokenKind TokenKind { get; }
        public object Value { get; }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.SyntaxToken;

        public SyntaxToken(SyntaxTokenKind kind, TextLocation location, object value, bool isValid = true) : base(isValid, location)
        {
            TokenKind = kind;
            Value = value;
        }
    }
}