using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(FunctionSymbol mainFunction, ImmutableArray<GlobalVariableSymbol> globals, ImmutableArray<FunctionSymbol> functions, DiagnosticReport diagnostics)
        {
            MainFunction = mainFunction;
            Globals = globals;
            Functions = functions;
            Diagnostics = diagnostics;
        }

        public FunctionSymbol MainFunction { get; }
        public ImmutableArray<GlobalVariableSymbol> Globals { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public DiagnosticReport Diagnostics { get; }
    }

    internal sealed class BoundProgram : BoundNode
    {
        public BoundProgram(BoundGlobalScope globalScope, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, DiagnosticReport diagnostics, bool isValid) : base(isValid)
        {
            GlobalScope = globalScope;
            Functions = functions;
            Diagnostics = diagnostics;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundProgram;
        public ImmutableArray<GlobalVariableSymbol> GlobalVariables { get; }
        public BoundGlobalScope GlobalScope { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public DiagnosticReport Diagnostics { get; }
        
    }
}
