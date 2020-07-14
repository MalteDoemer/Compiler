using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.Symbols
{
    internal static class BuiltInFunctions
    {
        public static readonly FunctionSymbol Print = new FunctionSymbol("print", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("text", TypeSymbol.Any)), TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new FunctionSymbol("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        public static readonly FunctionSymbol Len = new FunctionSymbol("len", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("str", TypeSymbol.String)), TypeSymbol.Int);
        public static readonly FunctionSymbol Clear = new FunctionSymbol("clear", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
        public static readonly FunctionSymbol Exit = new FunctionSymbol("exit", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("exitCode", TypeSymbol.Int)), TypeSymbol.Void);
        public static readonly FunctionSymbol Random = new FunctionSymbol("rand", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("start", TypeSymbol.Int), new ParameterSymbol("stop", TypeSymbol.Int)), TypeSymbol.Int);
        public static readonly FunctionSymbol RandomFloat = new FunctionSymbol("randf", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Float);

        public static IEnumerable<FunctionSymbol> GetAll()
        {
            yield return Print;
            yield return Input;
            yield return Len;
            yield return Clear;
            yield return Exit;
            yield return Random;
            yield return RandomFloat;
        }
    }
}