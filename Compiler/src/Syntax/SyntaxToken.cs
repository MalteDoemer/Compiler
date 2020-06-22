using Compiler.Text;

namespace Compiler.Syntax
{
    public class SyntaxToken
    {
        public SyntaxTokenKind Kind { get; }
        public int Pos { get; }
        public int Lenght { get; }
        public object Value { get; }

        public TextSpan Span { get => new TextSpan(Pos, Lenght); }

        public SyntaxToken(SyntaxTokenKind kind, int pos, int len, object value)
        {
            Kind = kind;
            Pos = pos;
            Value = value;
            Lenght = len;
        }

        public override string ToString() => $"{Kind} at {Pos} : {Value}";
    }
}