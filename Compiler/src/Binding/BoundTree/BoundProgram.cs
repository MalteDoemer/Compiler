using System.Collections.Immutable;
using System.IO;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal class BoundProgram : BoundNode
    {
        public BoundProgram(BoundProgram previous, BoundBlockStatement globalStatements, ImmutableArray<VariableSymbol> globalVariables, FunctionSymbol mainFunction, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics, bool isValid)
        {
            Previous = previous;
            GlobalStatements = globalStatements;
            GlobalVariables = globalVariables;
            MainFunction = mainFunction;
            Functions = functions;
            Diagnostics = diagnostics;
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundProgram;
        public override bool IsValid { get; }
        public BoundProgram Previous { get; }
        public BoundBlockStatement GlobalStatements { get; }
        public ImmutableArray<VariableSymbol> GlobalVariables { get; }
        public FunctionSymbol MainFunction { get; }
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


    internal static class BoundTreePrinter
    {
        public static void WriteTo(this BoundNode node, TextWriter writer)
        {

        }



    }
}
