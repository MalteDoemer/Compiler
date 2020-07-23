using System;
using System.Collections.Generic;

namespace Compiler.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Int = new TypeSymbol("int");
        public static readonly TypeSymbol Float = new TypeSymbol("float");
        public static readonly TypeSymbol Bool = new TypeSymbol("bool");
        public static readonly TypeSymbol String = new TypeSymbol("str");
        public static readonly TypeSymbol Obj = new TypeSymbol("obj");
        public static readonly TypeSymbol Void = new TypeSymbol("void");
        public static readonly TypeSymbol Invalid = new TypeSymbol("$Invalid");

        private TypeSymbol(string name) : base(name)
        {
            Type = this;
            IsArray = false;
        }

        private TypeSymbol(TypeSymbol type) : base(type.Name + "[]")
        {
            Type = type;
            IsArray = true;
        }

        public TypeSymbol Type { get; }
        public bool IsArray { get; }

        public TypeSymbol CreateArray() => new TypeSymbol(this);

        public static TypeSymbol? Lookup(string name)
        {
            switch (name)
            {
                case "int": return Int;
                case "float": return Float;
                case "bool": return Bool;
                case "str": return String;
                case "obj": return Obj;
                case "void": return Void;
                default: return null;
            }
        }

        public override bool Equals(object? obj) => obj is TypeSymbol symbol && Name == symbol.Name;
        public override int GetHashCode() => HashCode.Combine(Name);
    }
}