namespace Compiler.Diagnostics
{
    public enum ErrorMessage
    {
        InvalidDecimalPoint,
        NeverClosedStringLiteral,
        NeverClosedCurlyBrackets,
        NeverClosedParenthesis,
        ExpectedToken,
        UnExpectedToken,
        UnresolvedIdentifier,
        IncompatibleTypes,
        UnsupportedBinaryOperator,
        UnsupportedUnaryOperator,
        VariableAlreadyDeclared,
        WrongAmountOfArguments,
        CannotBeVoid,
        MissingExplicitConversion,
        CannotConvert,
        InvalidStatement,
        DuplicatedParameters,
        FunctionAlreadyDeclared,
    }
}