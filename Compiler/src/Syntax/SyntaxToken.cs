using System.Collections.Immutable;
using Compiler.Text;

namespace Compiler.Syntax
{
    public class SyntaxToken
    {
        public SyntaxTokenKind Kind { get; }
        public object Value { get; }
        public TextLocation Location { get; }
        public bool IsValid { get; }

        public SyntaxToken(SyntaxTokenKind kind, TextLocation location, object value, bool isValid = true)
        {
            Kind = kind;
            Value = value;
            Location = location;
            IsValid = isValid;
        }
    }

    internal class InterpolatedString
    {
        public InterpolatedString(string text, ImmutableArray<string> sections)
        {
            Text = text;
            Sections = sections;
        }

        public string Text { get; }
        public ImmutableArray<string> Sections { get; }
    }
}