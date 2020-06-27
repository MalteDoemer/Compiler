using System.Collections.Immutable;
using Compiler.Binding;
using Compiler.Text;

namespace Compiler
{

    internal sealed class BoundCompilationUnit : BoundNode
    {
        public BoundCompilationUnit(BoundStatement statement, ImmutableArray<VariableSymbol> declaredVariables)
        {
            DeclaredVariables = declaredVariables;
            Statement = statement;
        }

        public BoundStatement Statement { get; }
        public ImmutableArray<VariableSymbol> DeclaredVariables { get; }
    }
}
