using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.Symbols
{
    internal static class BuiltInFunctions
    {
        public static readonly FunctionSymbol Print = new FunctionSymbol("print", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("text", TypeSymbol.String)), TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new FunctionSymbol("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        public static FunctionSymbol Clear = new FunctionSymbol("clear", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);

        public static IEnumerable<FunctionSymbol> GetAll()
        {
            yield return Print;
            yield return Input;
            yield return Clear;
        }
    }

    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType) : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
    }
}