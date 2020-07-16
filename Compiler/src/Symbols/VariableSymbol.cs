using System;

namespace Compiler.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        internal static readonly VariableSymbol Invalid = new GlobalVariableSymbol("$invalid", TypeSymbol.ErrorType);

        public VariableSymbol(string name, TypeSymbol type, bool isConst = false) : base(name)
        {
            Type = type;
            IsConst = isConst;
        }

        public TypeSymbol Type { get; }
        public bool IsConst { get; }

        public override bool Equals(object obj) => obj is VariableSymbol symbol && Name == symbol.Name && Type == symbol.Type && IsConst == symbol.IsConst;
        public override int GetHashCode() => HashCode.Combine(Name, Type, IsConst);
    }
}