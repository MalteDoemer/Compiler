using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal class BoundProgram : BoundNode
    {
        public BoundProgram(BoundProgram previous, ImmutableArray<VariableSymbol> globalVariables, ImmutableArray<BoundStatement> globalStatements, FunctionSymbol mainFunction, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, DiagnosticReport diagnostics, bool isValid) : base(isValid)
        {
            Previous = previous;
            GlobalVariables = globalVariables;
            GlobalStatements = globalStatements;
            MainFunction = mainFunction;
            Functions = functions;
            Diagnostics = diagnostics;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundProgram;
        public BoundProgram Previous { get; }
        public FunctionSymbol MainFunction { get; }
        public ImmutableArray<VariableSymbol> GlobalVariables { get; }
        public ImmutableArray<BoundStatement> GlobalStatements { get; }
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
