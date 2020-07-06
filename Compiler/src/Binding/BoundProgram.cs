using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal class BoundProgram
    {
        public BoundProgram(BoundProgram previous, BoundBlockStatement globalStatements, ImmutableArray<VariableSymbol> globalVariables, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics)
        {
            Previous = previous;
            GlobalStatements = globalStatements;
            GlobalVariables = globalVariables;
            Functions = functions;
            Diagnostics = diagnostics;
        }

        public BoundProgram Previous { get; }
        public BoundBlockStatement GlobalStatements { get; }
        public ImmutableArray<VariableSymbol> GlobalVariables { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public BoundBlockStatement GetFunctionBody(FunctionSymbol symbol)
        {
            if (Functions.ContainsKey(symbol))
                return Functions[symbol];
            else
                return Previous.GetFunctionBody(symbol);
        }
    }
}
