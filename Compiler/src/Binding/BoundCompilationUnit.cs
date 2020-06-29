using System.Collections.Immutable;
using Compiler.Binding;
using Compiler.Symbols;
using Compiler.Text;

namespace Compiler
{

    internal sealed class BoundCompilationUnit : BoundNode
    {
        public BoundCompilationUnit(BoundStatement statement, ImmutableArray<VariableSymbol> declaredVariables, ImmutableArray<FunctionSymbol> declaredFunctions)
        {
            DeclaredVariables = declaredVariables;
            DeclaredFunctions = declaredFunctions;
            Statement = statement;
        }

        public BoundStatement Statement { get; }
        public ImmutableArray<VariableSymbol> DeclaredVariables { get; }
        public ImmutableArray<FunctionSymbol> DeclaredFunctions { get; }
    }
}
