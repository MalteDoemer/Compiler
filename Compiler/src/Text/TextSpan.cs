using System;

namespace Compiler.Text
{
    public struct TextSpan
    {
        private readonly static TextSpan undefined = new TextSpan(-10,-10);

        public static TextSpan Undefined { get => undefined; }


        private TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public static TextSpan FromBounds(int start, int end) => new TextSpan(start, end - start);
        public static TextSpan FromLength(int start, int length) => new TextSpan(start, length);

        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;

        public override bool Equals(object obj) => obj is TextSpan span && Start == span.Start && Length == span.Length;
        public override int GetHashCode() => HashCode.Combine(Start, Length, End);

        public static TextSpan operator +(TextSpan l, TextSpan r) => FromBounds(l.Start, r.End);
        public static bool operator ==(TextSpan l, TextSpan r) => l.Start == r.Start && l.Length == r.Length;
        public static bool operator !=(TextSpan l, TextSpan r) => l.Start != r.Start || l.Length != r.Length;

        public override string ToString() => $"{Start}..{End}";
    }
}