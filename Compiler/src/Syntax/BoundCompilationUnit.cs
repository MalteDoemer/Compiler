using System.Collections.Immutable;
using Compiler.Binding;
using Compiler.Text;

namespace Compiler
{
    internal sealed class BoundCompilationUnit : BoundNode
    {
        public BoundCompilationUnit(BoundStatement statement, ImmutableArray<VariableSymbol> declaredVariables, TextSpan span)
        {
            DeclaredVariables = declaredVariables;
            Statement = statement;
            Span = span;
        }

        public override TextSpan Span { get; }
        public BoundStatement Statement { get; }
        public ImmutableArray<VariableSymbol> DeclaredVariables { get; }
    }
}
