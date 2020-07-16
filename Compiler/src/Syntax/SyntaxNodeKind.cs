namespace Compiler.Syntax
{
    public enum SyntaxNodeKind
    {
        CompilationUnitSyntax,
        FunctionDeclarationSyntax,
        GlobalStatementSynatx,

        ParameterSyntax,
        TypeClauseSyntax,

        LiteralExpressionSyntax,
        VariableExpressionSyntax,
        UnaryExpressionSyntax,
        BinaryExpressionSyntax,
        CallExpressionSyntax,
        AssignmentExpressionSyntax,
        AdditionalAssignmentExpressionSyntax,
        PostIncDecExpressionSyntax,

        BlockStatmentSyntax,
        ExpressionStatementSyntax,
        VariableDeclarationStatementSyntax,
        IfStatementSyntax,
        ElseStatementSyntax,
        WhileStatementSyntax,
        ForStatementSyntax,
        DoWhileStatementSyntax,
        BreakStatementSyntax,
        ContinueStatementSyntax,
        ReturnStatementSyntax,
        InterpolatedStringSyntax,
    }
}