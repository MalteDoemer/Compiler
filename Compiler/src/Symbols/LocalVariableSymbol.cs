using System;
using Compiler.Binding;

namespace Compiler.Symbols
{
    public sealed class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, TypeSymbol type, bool isReadonly = false, BoundConstant? constant = null) : base(name, type, isReadonly, constant)
        {
        }
    }
}