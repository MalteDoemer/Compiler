using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Symbols
{
    public abstract class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Int = new PrimitiveTypeSymbol("int");
        public static readonly TypeSymbol Float = new PrimitiveTypeSymbol("float");
        public static readonly TypeSymbol Bool = new PrimitiveTypeSymbol("bool");
        public static readonly TypeSymbol String = new PrimitiveTypeSymbol("str");
        public static readonly TypeSymbol Obj = new PrimitiveTypeSymbol("obj");
        public static readonly TypeSymbol Void = new PrimitiveTypeSymbol("void");
        public static readonly TypeSymbol Invalid = new PrimitiveTypeSymbol("$Invalid");

        protected TypeSymbol(string name) : base(name)
        {
        }

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
        
        
        public static IEnumerable<TypeSymbol> GetPrimitiveTypes()
        {
            yield return Int;
            yield return Float;
            yield return Bool;
            yield return String;
            yield return Obj;
            yield return Void;   
        }
        
        public override bool Equals(object? obj) => obj is TypeSymbol type && type.Name == Name;
        public override int GetHashCode() => HashCode.Combine(Name);

        public static bool operator ==(TypeSymbol? l, TypeSymbol? r)
        {
            if (l is null && r is null)
                return true;
            else if (l is null || r is null)
                return false;
            else return l.Name == r.Name;
        }
        public static bool operator !=(TypeSymbol? l, TypeSymbol? r) => !(l == r);

    }

    public sealed class PrimitiveTypeSymbol : TypeSymbol
    {
        internal PrimitiveTypeSymbol(string name) : base(name)
        {
        }
    }


    public sealed class ArrayTypeSymbol : TypeSymbol
    {
        internal ArrayTypeSymbol(TypeSymbol type) : base(type.Name + "[]")
        {
            UnderlyingType = type;
            if (type is ArrayTypeSymbol array)
                Rank = array.Rank + 1;
            else Rank = 1;
        }

        public TypeSymbol UnderlyingType { get; }
        public int Rank { get; }

        public TypeSymbol BaseType
        {
            get
            {
                var type = UnderlyingType;

                while (type is ArrayTypeSymbol array)
                    type = array.UnderlyingType;

                return type;
            }
        }
    }
}