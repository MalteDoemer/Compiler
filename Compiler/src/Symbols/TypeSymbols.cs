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
        public static readonly TypeSymbol ErrorType = new TypeSymbol("Error-Type");

        private TypeSymbol(string name) : base(name)
        {

        }

        public static TypeSymbol Lookup(string name)
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
    }
}