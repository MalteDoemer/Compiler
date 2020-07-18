using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal class BoundProgram : BoundNode
    {
        public BoundProgram(ImmutableArray<GlobalVariableSymbol> globalVariables, FunctionSymbol mainFunction, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, DiagnosticReport diagnostics, bool isValid) : base(isValid)
        {
            GlobalVariables = globalVariables;
            MainFunction = mainFunction;
            Functions = functions;
            Diagnostics = diagnostics;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundProgram;
        public FunctionSymbol MainFunction { get; }
        public ImmutableArray<GlobalVariableSymbol> GlobalVariables { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public DiagnosticReport Diagnostics { get; }
    }
}
