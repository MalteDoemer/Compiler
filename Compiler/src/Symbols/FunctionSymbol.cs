using System.Collections.Immutable;
using Compiler.Syntax;

namespace Compiler.Symbols
{

    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType) : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType, BlockStatmentSyntax body) : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
        internal BlockStatmentSyntax Body { get; }
    }
}