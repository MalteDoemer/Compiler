using System.Collections.Immutable;
using Mono.Cecil;
using Compiler.Diagnostics;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal class BoundProgram : BoundNode
    {
        public BoundProgram(ImmutableArray<VariableSymbol> globalVariables, FunctionSymbol? globalFunction, FunctionSymbol? mainFunction, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, AssemblyDefinition mainAssembly, ImmutableArray<AssemblyDefinition> referencAssemblies, ImmutableDictionary<TypeSymbol, TypeReference> resolvedTypes, DiagnosticReport diagnostics, bool isValid) : base(isValid)
        {
            GlobalVariables = globalVariables;
            GlobalFunction = globalFunction;
            MainFunction = mainFunction;
            Functions = functions;
            MainAssembly = mainAssembly;
            ReferencAssemblies = referencAssemblies;
            ResolvedTypes = resolvedTypes;
            Diagnostics = diagnostics;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundProgram;
        public FunctionSymbol? MainFunction { get; }
        public FunctionSymbol? GlobalFunction { get; }
        public ImmutableArray<VariableSymbol> GlobalVariables { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public ImmutableDictionary<TypeSymbol, TypeReference> ResolvedTypes { get; }
        public AssemblyDefinition MainAssembly { get; }
        public ImmutableArray<AssemblyDefinition> ReferencAssemblies { get; }
        public DiagnosticReport Diagnostics { get; }
    }
}
