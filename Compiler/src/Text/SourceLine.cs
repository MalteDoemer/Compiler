namespace Compiler.Text
{
    public sealed class SourceLine
    {
        public SourceLine(SourceText parent, TextSpan span, TextSpan spanWithLineBreak)
        {
            Parent = parent;
            Span = span;
            SpanWithLineBreak = spanWithLineBreak;
        }

        public SourceText Parent { get; }
        public TextSpan Span { get; }
        public TextSpan SpanWithLineBreak { get; }
        public override string ToString() => Parent.ToString(Span);
    }
}