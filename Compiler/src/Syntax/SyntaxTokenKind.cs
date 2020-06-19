namespace Compiler.Syntax
{
    internal enum SyntaxTokenKind
    {
        End,
        Invalid,
        Error,
        Number,
        String,
        Identifier,

        Plus,
        Minus,
        Star,
        Slash,
        StarStar,
        SlashSlah,

        True,
        False,
        Null
    }
}