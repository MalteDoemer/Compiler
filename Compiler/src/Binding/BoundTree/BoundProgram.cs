using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal class BoundProgram : BoundNode
    {
        public BoundProgram(BoundProgram previous, ImmutableArray<GlobalVariableSymbol> globalVariables, FunctionSymbol mainFunction, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, DiagnosticReport diagnostics, bool isValid)
        {
            Previous = previous;
            GlobalVariables = globalVariables;
            MainFunction = mainFunction;
            Functions = functions;
            Diagnostics = diagnostics;
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundProgram;
        public override bool IsValid { get; }
        public BoundProgram Previous { get; }
        public FunctionSymbol MainFunction  { get; }
        public ImmutableArray<GlobalVariableSymbol> GlobalVariables { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public DiagnosticReport Diagnostics { get; }

        public BoundBlockStatement GetFunctionBody(FunctionSymbol symbol)
        {
            if (Functions.ContainsKey(symbol))
                return Functions[symbol];
            else
                return Previous.GetFunctionBody(symbol);
        }

        public IEnumerable<FunctionSymbol> GetFunctionSymbols()
        {
            foreach (var func in Functions.Keys)
                yield return func;

            var pre = Previous;

            while (pre != null)
            {
                foreach (var func in pre.Functions.Keys)
                    yield return func;
                pre = pre.Previous;
            }
        }
    }
}
