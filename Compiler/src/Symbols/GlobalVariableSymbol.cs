using System;
using Compiler.Binding;

namespace Compiler.Symbols
{
    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        internal GlobalVariableSymbol(string name, TypeSymbol type, bool isReadOnly = false, BoundConstant constant = null) : base(name, type, isReadOnly, constant)
        {
        }
    }
}