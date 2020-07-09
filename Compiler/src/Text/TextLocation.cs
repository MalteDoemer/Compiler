namespace Compiler.Text
{
    public struct TextLocation
    {
        public TextLocation(SourceText text, TextSpan span)
        {
            this.text = text;
            this.span = span;
        }

        public SourceText text { get; }
        public TextSpan span { get; }

        public int StartLine { get => text.GetLineNumber(span.Start); }
        public int EndLine { get => text.GetLineNumber(span.End); }
        public int StartCharacter { get => text.GetCharacterOffset(span.Start); }
        public int EndCharacter { get => text.GetCharacterOffset(span.End); }
    }
}