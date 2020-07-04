namespace Compiler.Syntax
{
    public enum SyntaxTokenKind
    {
        // Special
        EndOfFile,
        Invalid,
        Int,
        Float,
        String,
        Identifier,
        Comment,
        Space,

        // Single characters
        Equal,
        Plus,
        Minus,
        Star,
        Slash,
        Percentage,
        Tilde,
        Ampersand,
        Pipe,
        Hat,
        GreaterThan,
        LessThan,
        LParen,
        RParen,
        LCurly,
        RCurly,
        Bang,
        Comma,
        Colon,

        // Double characters
        GreaterEqual,
        LessEqual,
        StarStar,
        SlashSlah,
        EqualEqual,
        NotEqual,
        PipePipe,
        AmpersandAmpersand,
        PlusPlus,
        MinusMinus,
        PlusEqual,
        MinusEqual,
        StarEqual,
        SlashEqual,
        AmpersandEqual,
        PipeEqual,
        
        // Keywrods
        TrueKeyword,
        FalseKeyword,
        VarKeyword,
        FloatKeyword,
        BoolKeyword,
        StringKeyword,
        IntKeyword,
        AnyKeyword,
        VoidKeyword,

        // Control flow 
        IfKeyword,
        ElseKeyword,
        ForKeyword,
        WhileKeyword,
        DoKeyword,
    }
}