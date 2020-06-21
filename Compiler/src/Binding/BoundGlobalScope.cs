using System.Collections.Immutable;
using Compiler.Diagnostics;

namespace Compiler.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, DiagnosticBag bag, ImmutableArray<VariableSymbol> variables, BoundStatement statement)
        {
            Previous = previous;
            Bag = bag;
            Variables = variables;
            Statement = statement;
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Bag { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundStatement Statement { get; }
    }
}