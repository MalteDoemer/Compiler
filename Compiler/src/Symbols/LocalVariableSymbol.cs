using System;
using Compiler.Binding;

namespace Compiler.Symbols
{
    public sealed class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, TypeSymbol type, bool isConst = false, BoundConstant constant = null) : base(name, type, isConst)
        {
            Constant = constant;

            if (isConst && constant == null || !isConst && constant != null)
                throw new Exception("Variable is declared as const but has no constant value");
        }

        internal BoundConstant Constant { get; }
    }
}