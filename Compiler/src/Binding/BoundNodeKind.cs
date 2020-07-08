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
        BoundAssignementExpression,

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

        BoundInvalidExpression,
        BoundInvalidStatement,
    }
}
