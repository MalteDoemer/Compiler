using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.Symbols
{
    internal static class BuiltInFunctions
    {
        public static readonly FunctionSymbol Print = new FunctionSymbol("print", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("text", 0, TypeSymbol.Any)), TypeSymbol.Void);
        public static readonly FunctionSymbol PrintLine = new FunctionSymbol("println", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("text", 0, TypeSymbol.Any)), TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new FunctionSymbol("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        public static readonly FunctionSymbol Len = new FunctionSymbol("len", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("str", 0, TypeSymbol.String)), TypeSymbol.Int);
        public static readonly FunctionSymbol Clear = new FunctionSymbol("clear", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
        public static readonly FunctionSymbol Exit = new FunctionSymbol("exit", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("exitCode", 0, TypeSymbol.Int)), TypeSymbol.Void);
        public static readonly FunctionSymbol Random = new FunctionSymbol("rand", ImmutableArray.Create<ParameterSymbol>(new ParameterSymbol("start", 0, TypeSymbol.Int), new ParameterSymbol("stop", 1, TypeSymbol.Int)), TypeSymbol.Int);
        public static readonly FunctionSymbol RandomFloat = new FunctionSymbol("randf", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Float);

        public static IEnumerable<FunctionSymbol> GetAll()
        {
            yield return Print;
            yield return PrintLine;
            yield return Input;
            yield return Len;
            yield return Clear;
            yield return Exit;
            yield return Random;
            yield return RandomFloat;
        }
    }
}