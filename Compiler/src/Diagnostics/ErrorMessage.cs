namespace Compiler.Diagnostics
{
    public enum ErrorMessage
    {
        InvalidDecimalPoint,
        NeverClosedStringLiteral,
        NeverClosedCurlyBrackets,
        NeverClosedParenthesis,
        ExpectedToken,
        UnexpectedToken,
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
        InvalidGlobalStatement,
        CannotAssignToReadOnly,
        InvalidBreakOrContinue,
        ReturnOnlyInFunction,
        AllPathsMustReturn,
        InvalidReference,
        MissingRequiredType,
        AmbiguousRequiredType,
        MissingRequiredMethod,
        AmbiguousRequiredMethod,
        InvalidEscapeSequence,
        TypeNotFound,
        ArrayCreationMustHaveSize,
    }
}