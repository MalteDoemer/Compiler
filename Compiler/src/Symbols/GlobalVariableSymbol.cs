using System;
using Compiler.Binding;

namespace Compiler.Symbols
{
    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        internal GlobalVariableSymbol(string name, TypeSymbol type, bool isConst = false, BoundConstant constant = null) : base(name, type, isConst)
        {
            if (isConst)
                Constant = constant;
        }

        internal BoundConstant Constant { get; }
    }
}