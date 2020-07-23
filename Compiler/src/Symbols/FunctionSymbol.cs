using System.Collections.Immutable;
using Compiler.Syntax;

namespace Compiler.Symbols
{

    public sealed class FunctionSymbol : Symbol
    {
        internal static readonly FunctionSymbol Invalid = new FunctionSymbol("$invalid", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Invalid);

        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType) : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType, FunctionDeclarationSyntax syntax) : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Syntax = syntax;
        }


        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
        public FunctionDeclarationSyntax? Syntax { get; }
        public bool Exists { get => this != Invalid; }
    }
}