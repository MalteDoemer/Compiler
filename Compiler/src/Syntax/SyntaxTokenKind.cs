namespace Compiler.Syntax
{
    public enum SyntaxTokenKind
    {
        End,
        Invalid,
        Error,
        Int,
        Float,
        String,
        Identifier,

        True,
        False,
        Null,

        Plus,
        Minus,
        Star,
        Slash,
        StarStar,
        SlashSlah,
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