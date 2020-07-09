using System;
using System.Collections.Immutable;

namespace Compiler.Text
{
    public sealed class SourceText
    {
        public SourceText(string text)
        {
            Text = text;
            Lines = ParseLines(this, text);
        }

        public static implicit operator SourceText(string text) => new SourceText(text);

        public ImmutableArray<SourceLine> Lines { get; }
        public string Text { get; }
        public int Length => Text.Length;
        public char this[int i] { get => Text[i]; }

        public int GetLineIndex(int pos)
        {
            var lower = 0;
            var upper = Lines.Length - 1;

            while (lower <= upper)
            {
                var index = lower + (upper - lower) / 2;
                var start = Lines[index].Span.Start;

                if (pos == start) return index;
                else if (start > pos) upper = index - 1;
                else if (start < pos) lower = index + 1;
            }

            return lower - 1;
        }
        public int GetLineNumber(int pos)
        {
            var lower = 0;
            var upper = Lines.Length - 1;

            while (lower <= upper)
            {
                var index = lower + (upper - lower) / 2;
                var start = Lines[index].Span.Start;

                if (pos == start) return index;
                else if (start > pos) upper = index - 1;
                else if (start < pos) lower = index + 1;
            }

            return lower;
        }
        
        public int GetCharacterOffset(int pos) => pos - Lines[GetLineIndex(pos)].Span.Start;

        public override string ToString() => Text;
        public string ToString(TextSpan span) => ToString(span.Start, span.Length);
        public string ToString(int pos, int len)
        {
            if (pos < 0)
                throw new ArgumentOutOfRangeException("pos", $"Negative position: {pos}");
            else if (pos + len > Text.Length)
                throw new ArgumentOutOfRangeException("pos, len", $"Index out of bounds pos: {pos}, len: {len}");
            return Text.Substring(pos, len);
        }

        private static ImmutableArray<SourceLine> ParseLines(SourceText src, string text)
        {
            var result = ImmutableArray.CreateBuilder<SourceLine>();

            var pos = 0;
            var lineStart = 0;

            while (pos < text.Length)
            {
                var lineBreakLen = GetLineBreakLength(text, pos);

                if (lineBreakLen == 0) pos++;
                else
                {
                    AddLine(src, result, pos, lineStart, lineBreakLen);

                    pos += lineBreakLen;
                    lineStart = pos;
                }
            }

            if (pos >= lineStart)
                AddLine(src, result, pos, lineStart, 0);

            return result.ToImmutable();
        }
        private static void AddLine(SourceText src, ImmutableArray<SourceLine>.Builder result, int pos, int lineStart, int lineBreakLen)
        {
            var span = TextSpan.FromLength(lineStart, pos);
            var spanWithLineBreak = TextSpan.FromLength(lineStart, pos + lineBreakLen);
            result.Add(new SourceLine(src, span, spanWithLineBreak));
        }
        private static int GetLineBreakLength(string text, int pos)
        {
            char c = '\0';
            char n = '\0';
            if (pos < text.Length)
                c = text[pos];
            if (pos + 1 < text.Length)
                n = text[pos + 1];


            if (c == '\r' && n == '\n') return 2;
            if (c == '\r' || c == '\n') return 1;
            return 0;
        }
    }
}