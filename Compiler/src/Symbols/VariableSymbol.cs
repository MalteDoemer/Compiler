using System;
using Compiler.Binding;

namespace Compiler.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        //internal static readonly VariableSymbol Invalid = new GlobalVariableSymbol("$invalid", TypeSymbol.ErrorType);

        internal VariableSymbol(string name, TypeSymbol type, bool isReadonly = false, BoundConstant constant = null) : base(name)
        {
            Type = type;
            IsReadOnly = isReadonly;
            if (isReadonly)
                Constant = constant;
        }

        public TypeSymbol Type { get; }
        public bool IsReadOnly { get; }
        internal BoundConstant Constant { get; }

        public override bool Equals(object obj) => obj is VariableSymbol symbol && Name == symbol.Name && Type == symbol.Type && IsReadOnly == symbol.IsReadOnly;
        public override int GetHashCode() => HashCode.Combine(Name, Type, IsReadOnly);
    }
}