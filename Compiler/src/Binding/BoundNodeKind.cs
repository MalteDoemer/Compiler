namespace Compiler.Binding
{
    internal enum BoundNodeKind
    {
        BoundProgram,
        BoundLiteralExpression,
        BoundVariableExpression,
        BoundUnaryExpression,
        BoundBinaryExpression,
        BoundCallExpression,
        BoundConversionExpression,
        BoundAssignmentExpression,
        BoundBlockStatement,
        BoundExpressionStatement,
        BoundVariableDeclarationStatement,
        BoundIfStatement,
        BoundForStatement,
        BoundWhileStatement,
        BoundDoWhileStatement,
        BoundConditionalGotoStatement,
        BoundGotoStatement,
        BoundLabelStatement,
        BoundReturnStatement,
        BoundNopStatement,
        BoundInvalidExpression,
    }
}
