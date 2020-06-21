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

        public ImmutableArray<SourceLine> Lines { get; }
        public string Text { get; }
        public int Length => Text.Length;
        public char this[int i] { get => Text[i]; }

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

            return lower - 1;

        }


        public override string ToString() => Text;
        public string ToString(TextSpan span) => Text.Substring(span.Start, span.Lenght);
        public string ToString(int pos, int len) => Text.Substring(pos, len);


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

            if (pos > lineStart)
                AddLine(src, result, pos, lineStart, 0);

            return result.ToImmutable();
        }
        private static void AddLine(SourceText src, ImmutableArray<SourceLine>.Builder result, int pos, int lineStart, int lineBreakLen)
        {
            var span = new TextSpan(lineStart, pos);
            var spanWithLineBreak = new TextSpan(lineStart, pos + lineBreakLen);
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