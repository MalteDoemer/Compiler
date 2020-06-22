using System;

namespace Compiler.Text
{
    public struct TextSpan
    {
        public static TextSpan Invalid { get => invalid; }
        private readonly static TextSpan invalid = new TextSpan(-10, -10);


        public TextSpan(int start, int lenght)
        {
            Start = start;
            Lenght = lenght;
        }

        public static TextSpan FromBounds(int start, int end) => new TextSpan(start, end - start);

        public override bool Equals(object obj)
        {
            return obj is TextSpan span &&
                   Start == span.Start &&
                   Lenght == span.Lenght;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, Lenght, End);
        }

        public static TextSpan operator +(TextSpan l, TextSpan r) => FromBounds(l.Start, r.End);
        public static bool operator ==(TextSpan l, TextSpan r) => l.Start == r.Start && l.Lenght == r.Lenght;
        public static bool operator !=(TextSpan l, TextSpan r) => l.Start != r.Start || l.Lenght != r.Lenght;

        public int Start { get; }
        public int Lenght { get; }
        public int End => Start + Lenght;

        public override string ToString() => $"{Start}..{End}";
    }
}