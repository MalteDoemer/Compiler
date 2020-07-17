using System;
using Compiler.Binding;

namespace Compiler.Symbols
{
    public sealed class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, TypeSymbol type, bool isConst = false, BoundConstant constant = null) : base(name, type, isConst, constant)
        {
        }
    }
}