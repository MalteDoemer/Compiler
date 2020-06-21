using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, VariableSymbol> variables;

        public BoundScope Parent { get; }

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
            variables = new Dictionary<string, VariableSymbol>();
        }

        public bool TryLookUp(string identifier, out VariableSymbol value)
        {
            if (variables.TryGetValue(identifier, out value))
                return true;

            if (Parent == null) return false;

            return Parent.TryLookUp(identifier, out value);

        }

        public bool TryDeclare(VariableSymbol variable)
        {
            if (variables.ContainsKey(variable.Identifier))
                return false;
            variables.Add(variable.Identifier, variable);
            return true;
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            return variables.Values.ToImmutableArray();
        }
    }
}