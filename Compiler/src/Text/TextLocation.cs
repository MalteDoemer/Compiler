using System;
using System.Collections.Generic;

namespace Compiler.Text
{
    public struct TextLocation
    {
        public static readonly TextLocation Undefined = new TextLocation(new SourceText(string.Empty, null), TextSpan.Undefined);

        public TextLocation(SourceText text, TextSpan span)
        {
            Text = text;
            Span = span;
        }
        public TextLocation(SourceText text, int start, int len)
        {
            Text = text;
            Span = TextSpan.FromLength(start, len);
        }

        public SourceText Text { get; }
        public TextSpan Span { get; }

        public int StartLine { get => Text.GetLineNumber(Span.Start); }
        public int EndLine { get => Text.GetLineNumber(Span.End); }
        public int StartCharacter { get => Text.GetCharacterOffset(Span.Start); }
        public int EndCharacter { get => Text.GetCharacterOffset(Span.End); }

        public override bool Equals(object obj) => obj is TextLocation location && Text == location.Text && Span == location.Span;
        public override int GetHashCode() => HashCode.Combine(Text, Span, StartLine, EndLine, StartCharacter, EndCharacter);

        public static bool operator ==(TextLocation l, TextLocation r) => l.Text == r.Text && l.Span == r.Span;
        public static bool operator !=(TextLocation l, TextLocation r) => l.Text != r.Text || r.Span != r.Span;
    }
}