namespace Compiler.Syntax
{
    public enum SyntaxTokenKind
    {
        End,
        Invalid,
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
        LessThan,
        GreaterThan,
        EqualEqual,
        NotEqual,
        LessEqual,
        GreaterEqual,
        PipePipe,
        AmpersandAmpersand,
        Equal,
        LParen,
        RParen,
        LCurly,
        RCurly,
        Bang,
        
        FloatKeyword,
        BoolKeyword,
        StringKeyword,
        IntKeyword,
        VarKey,
        IfKeyword,
        ElseKeyword,
    }
}