using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, VariableSymbol> variables;
        private Dictionary<string, FunctionSymbol> functions;

        public BoundScope Parent { get; }

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
            variables = new Dictionary<string, VariableSymbol>();
            functions = new Dictionary<string, FunctionSymbol>();
        }

        public bool TryLookUpVariable(string identifier, out VariableSymbol value)
        {
            if (variables.TryGetValue(identifier, out value))
                return true;
            else if (Parent == null)
            {
                value = VariableSymbol.Invalid;
                return false;
            }
            return Parent.TryLookUpVariable(identifier, out value);
        }

        public bool TryDeclareVariable(VariableSymbol variable)
        {
            if (variables.ContainsKey(variable.Name))
                return false;
            variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryLookUpFunction(string identifier, out FunctionSymbol value)
        {
            if (functions.TryGetValue(identifier, out value))
                return true;
            else if (Parent == null)
            {
                value = FunctionSymbol.Invalid;
                return false;
            }
            return Parent.TryLookUpFunction(identifier, out value);
        }

        public bool TryDeclareFunction(FunctionSymbol function)
        {
            if (functions.ContainsKey(function.Name))
                return false;
            functions.Add(function.Name, function);
            return true;
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables() => variables.Values.ToImmutableArray();
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => functions.Values.ToImmutableArray();
    }
}