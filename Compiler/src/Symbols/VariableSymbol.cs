using System;
using System.Collections.Generic;

namespace Compiler.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        internal static readonly VariableSymbol Invalid = new GlobalVariableSymbol("$invalid", TypeSymbol.ErrorType);

        public VariableSymbol(string name, TypeSymbol type, VariableModifier modifiers) : base(name)
        {
            Type = type;
            Modifiers = modifiers;
        }

        public TypeSymbol Type { get; }
        public VariableModifier Modifiers { get; }

        public override bool Equals(object obj) => obj is VariableSymbol symbol && Name == symbol.Name && Type == symbol.Type && Modifiers == symbol.Modifiers;
        public override int GetHashCode() => HashCode.Combine(Name, Type, Modifiers);
    }
}