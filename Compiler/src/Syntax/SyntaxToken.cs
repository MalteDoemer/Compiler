using Compiler.Text;

namespace Compiler.Syntax
{
    public class SyntaxToken
    {
        public SyntaxTokenKind Kind { get; }
        public object Value { get; }
        public TextSpan Span { get; }
        public bool IsValid { get; }

        public SyntaxToken(SyntaxTokenKind kind, int pos, int len, object value, bool isValid = true)
        {
            Kind = kind;
            Value = value;
            Span = TextSpan.FromLength(pos, len);
            IsValid = isValid;
        }

        public override string ToString() => $"{Kind} at {Span.Start} : {Value}";
    }
}