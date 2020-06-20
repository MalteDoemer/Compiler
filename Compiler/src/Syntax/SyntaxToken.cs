namespace Compiler.Syntax
{
    public class SyntaxToken
    {
        public readonly SyntaxTokenKind kind;
        public readonly int pos;
        public readonly dynamic value;

        public SyntaxToken(SyntaxTokenKind kind, int pos, dynamic value)
        {
            this.kind = kind;
            this.pos = pos;
            this.value = value;
        }

        public override string ToString() => $"{kind} at {pos} : {value}";
    }
}