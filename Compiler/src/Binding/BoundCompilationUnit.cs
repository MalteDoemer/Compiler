using System.Collections.Immutable;
using Compiler.Binding;
using Compiler.Text;

namespace Compiler
{

    internal sealed class BoundCompilationUnit : BoundNode
    {
        public BoundCompilationUnit(BoundInvalidStatement statement, TextSpan span)
        {  
            IsValid = false;
            Span = span;
            DeclaredVariables = ImmutableArray<VariableSymbol>.Empty;
            Statement = statement;
        }

        public BoundCompilationUnit(BoundStatement statement, ImmutableArray<VariableSymbol> declaredVariables, TextSpan span)
        {
            DeclaredVariables = declaredVariables;
            Statement = statement;
            Span = span;
            IsValid = true;
        }

        public override TextSpan Span { get; }
        public BoundStatement Statement { get; }
        public ImmutableArray<VariableSymbol> DeclaredVariables { get; }
        public bool IsValid{ get; }
    }
}
