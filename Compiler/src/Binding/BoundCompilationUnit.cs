using System.Collections.Immutable;
using Compiler.Binding;
using Compiler.Text;

namespace Compiler
{

    internal sealed class BoundCompilationUnit : BoundNode
    {
        public BoundCompilationUnit(BoundInvalidStatement statement)
        {  
            IsValid = false;
            DeclaredVariables = ImmutableArray<VariableSymbol>.Empty;
            Statement = statement;
        }

        public BoundCompilationUnit(BoundStatement statement, ImmutableArray<VariableSymbol> declaredVariables)
        {
            DeclaredVariables = declaredVariables;
            Statement = statement;
            IsValid = true;
        }

        public BoundStatement Statement { get; }
        public ImmutableArray<VariableSymbol> DeclaredVariables { get; }
        public bool IsValid{ get; }
    }
}
