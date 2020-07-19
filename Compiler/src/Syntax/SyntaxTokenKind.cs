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

        // Brackets
        LParen,
        RParen,
        LCurly,
        RCurly,

        // Sperators
        Comma,
        Colon,

        // Type keywords
        FloatKeyword,
        BoolKeyword,
        StringKeyword,
        IntKeyword,
        AnyKeyword,
        VoidKeyword,

        // Declaring keywords
        VarKeyword,
        ConstKeyword,
        FunctionDefinitionKeyword,

        // Control flow keywords
        IfKeyword,
        ElseKeyword,
        ForKeyword,
        WhileKeyword,
        DoKeyword,
        ContinueKeyword,
        BreakKewyword,
        ReturnKeyword,
    }
}