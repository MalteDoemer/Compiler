using System.Collections.Immutable;
using Compiler.Diagnostics;

namespace Compiler.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, DiagnosticBag bag, ImmutableArray<VariableSymbol> variables, BoundExpression expr)
        {
            Previous = previous;
            Bag = bag;
            Variables = variables;
            Expr = expr;
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Bag { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundExpression Expr { get; }
    }
}