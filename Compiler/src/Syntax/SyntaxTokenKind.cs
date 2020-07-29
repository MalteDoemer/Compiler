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
        
        True,
        False,

        //Single operators
        Plus,
        Minus,
        Star,
        Slash,
        Percentage,
        Ampersand,
        Pipe,
        Hat,
        Tilde,
        Bang,
        QuestionMark,


        // Double operators
        StarStar,
        SlashSlah,
        PlusPlus,
        MinusMinus,
        PipePipe,
        AmpersandAmpersand,

        // Comparison operators
        LessThan,
        GreaterThan,
        EqualEqual,
        NotEqual,
        LessEqual,
        GreaterEqual,

        // Assignment operators
        Equal,
        PlusEqual,
        MinusEqual,
        StarEqual,
        SlashEqual,
        AmpersandEqual,
        PipeEqual,
        
        // Keyword operators
        NewKeyWord,

        // Brackets
        LParen,
        RParen,
        LCurly,
        RCurly,
        LSquare,
        RSquare,

        // Sperators
        Comma,
        Colon,

        // Type keywords
        FloatKeyword,
        BoolKeyword,
        StringKeyword,
        IntKeyword,
        ObjKeyword,
        VoidKeyword,

        // Declaring keywords
        VarKeyword,
        LetKeyword,
        FuncKeyword,

        // Control flow keywords
        IfKeyword,
        ElseKeyword,
        ForKeyword,
        WhileKeyword,
        DoKeyword,
        ContinueKeyword,
        BreakKewyword,
        ReturnKeyword,
        SwitchKeyword,
        CaseKeyword,
        DefaultKeyword,
    }
}