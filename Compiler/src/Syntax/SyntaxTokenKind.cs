namespace Compiler.Syntax
{
    internal enum SyntaxTokenKind
    {
        End,
        Invalid,
        Error,
        Int,
        Float,
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
        Null,
        LParen,
        RParen,
        LessThan,
        GreaterThan,
        Bang,
        EqualEqual,
        NotEqual,
        LessEqual,
        GreaterEqual,
        AmpersandAmpersand,
        PipePipe
    }
}