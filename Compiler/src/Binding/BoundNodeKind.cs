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
        BoundInvalidExpression,
        BoundNewArray,
        BoundTernaryExpression,
        BoundStatementExpression,
        
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
    }
}
