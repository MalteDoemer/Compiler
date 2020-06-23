using Compiler.Text;

namespace Compiler.Syntax
{
    public class SyntaxToken
    {
        public SyntaxTokenKind Kind { get; }
        public object Value { get; }

        public TextSpan Span { get; }

        public SyntaxToken(SyntaxTokenKind kind, int pos, int len, object value)
        {
            Kind = kind;
            Value = value;
            Span = new TextSpan(pos, len);
        }

        public override string ToString() => $"{Kind} at {Span.Start} : {Value}";
    }
}