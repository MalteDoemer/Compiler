using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal class BoundProgram : BoundNode
    {
        public BoundProgram(ImmutableArray<VariableSymbol> globalVariables, FunctionSymbol globalFunction, FunctionSymbol mainFunction, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, DiagnosticReport diagnostics, bool isValid) : base(isValid)
        {
            GlobalVariables = globalVariables;
            GlobalFunction = globalFunction;
            MainFunction = mainFunction;
            Functions = functions;
            Diagnostics = diagnostics;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundProgram;
        public FunctionSymbol MainFunction { get; }
        public ImmutableArray<VariableSymbol> GlobalVariables { get; }
        public FunctionSymbol GlobalFunction { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public DiagnosticReport Diagnostics { get; }
    }
}
